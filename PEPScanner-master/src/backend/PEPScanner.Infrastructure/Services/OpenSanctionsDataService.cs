using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO.Compression;

namespace PEPScanner.Infrastructure.Services
{
    public interface IOpenSanctionsDataService
    {
        Task<bool> DownloadAndUpdateDataAsync();
        Task<List<OpenSanctionsEntity>> SearchEntitiesAsync(string query, int limit = 10);
        Task<OpenSanctionsEntity?> GetEntityByIdAsync(string entityId);
        Task<List<OpenSanctionsEntity>> MatchPersonAsync(string name, DateTime? dateOfBirth = null, string? nationality = null);
        Task<List<OpenSanctionsEntity>> MatchOrganizationAsync(string name, string? country = null);
        Task<DateTime?> GetLastUpdateTimeAsync();
        Task<int> GetTotalEntitiesCountAsync();
    }

    public class OpenSanctionsDataService : IOpenSanctionsDataService
    {
        private readonly PepScannerDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenSanctionsDataService> _logger;
        private readonly IConfiguration _configuration;
        
        // OpenSanctions provides free bulk data downloads
        private const string OPENSANCTIONS_DATA_URL = "https://data.opensanctions.org/datasets/latest/default/entities.ftm.json";
        private const string OPENSANCTIONS_TARGETS_URL = "https://data.opensanctions.org/datasets/latest/targets/entities.ftm.json";

        public OpenSanctionsDataService(
            PepScannerDbContext context,
            HttpClient httpClient,
            ILogger<OpenSanctionsDataService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> DownloadAndUpdateDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting OpenSanctions data download and update");

                // Check if we should update (e.g., daily)
                var lastUpdate = await GetLastUpdateTimeAsync();
                var updateInterval = TimeSpan.FromDays(1); // Update daily
                
                if (lastUpdate.HasValue && DateTime.UtcNow - lastUpdate.Value < updateInterval)
                {
                    _logger.LogInformation("OpenSanctions data is up to date. Last update: {LastUpdate}", lastUpdate);
                    return true;
                }

                // Download the targets dataset (smaller, contains sanctioned entities)
                var downloadSuccess = await DownloadAndProcessDatasetAsync(OPENSANCTIONS_TARGETS_URL, "targets");
                
                if (downloadSuccess)
                {
                    // Update the last update timestamp
                    await UpdateLastUpdateTimeAsync();
                    _logger.LogInformation("OpenSanctions data update completed successfully");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading and updating OpenSanctions data");
                return false;
            }
        }

        private async Task<bool> DownloadAndProcessDatasetAsync(string url, string datasetName)
        {
            try
            {
                _logger.LogInformation("Downloading OpenSanctions dataset: {DatasetName} from {Url}", datasetName, url);

                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                var processedCount = 0;
                var batchSize = 1000;
                var batch = new List<OpenSanctionsEntity>();

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var entity = ParseOpenSanctionsEntity(line);
                        if (entity != null)
                        {
                            batch.Add(entity);
                            
                            if (batch.Count >= batchSize)
                            {
                                await SaveBatchAsync(batch);
                                processedCount += batch.Count;
                                batch.Clear();
                                
                                if (processedCount % 10000 == 0)
                                {
                                    _logger.LogInformation("Processed {Count} entities", processedCount);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing entity line: {Line}", line.Substring(0, Math.Min(100, line.Length)));
                    }
                }

                // Save remaining batch
                if (batch.Any())
                {
                    await SaveBatchAsync(batch);
                    processedCount += batch.Count;
                }

                _logger.LogInformation("Completed processing {DatasetName}. Total entities processed: {Count}", datasetName, processedCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading and processing dataset: {DatasetName}", datasetName);
                return false;
            }
        }

        private OpenSanctionsEntity? ParseOpenSanctionsEntity(string jsonLine)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonLine);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("id", out var idElement) || 
                    !root.TryGetProperty("schema", out var schemaElement))
                {
                    return null;
                }

                var entity = new OpenSanctionsEntity
                {
                    Id = idElement.GetString() ?? "",
                    Schema = schemaElement.GetString() ?? "",
                    Name = GetPropertyValue(root, "caption") ?? "",
                    Aliases = JsonSerializer.Serialize(GetPropertyArray(root, "name")),
                    Countries = JsonSerializer.Serialize(GetPropertyArray(root, "country")),
                    Addresses = JsonSerializer.Serialize(GetPropertyArray(root, "address")),
                    Identifiers = JsonSerializer.Serialize(GetPropertyArray(root, "idNumber")),
                    Sanctions = JsonSerializer.Serialize(GetPropertyArray(root, "sanctions")),
                    Datasets = JsonSerializer.Serialize(GetPropertyArray(root, "datasets")),
                    Score = 1.0, // Default score for downloaded data
                    FirstSeen = ParseDate(GetPropertyValue(root, "first_seen")),
                    LastSeen = ParseDate(GetPropertyValue(root, "last_seen")),
                    LastChange = ParseDate(GetPropertyValue(root, "last_change"))
                };

                // Parse birth date for persons
                if (entity.Schema == "Person")
                {
                    entity.BirthDate = ParseDate(GetPropertyValue(root, "birthDate"));
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error parsing OpenSanctions entity JSON");
                return null;
            }
        }

        private string? GetPropertyValue(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty("properties", out var properties) &&
                properties.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.Array &&
                property.GetArrayLength() > 0)
            {
                return property[0].GetString();
            }
            return null;
        }

        private List<string> GetPropertyArray(JsonElement root, string propertyName)
        {
            var result = new List<string>();
            
            if (root.TryGetProperty("properties", out var properties) &&
                properties.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.EnumerateArray())
                {
                    var value = item.GetString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.Add(value);
                    }
                }
            }
            
            return result;
        }

        private DateTime? ParseDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return null;
            
            if (DateTime.TryParse(dateString, out var date))
            {
                return date;
            }
            
            return null;
        }

        private async Task SaveBatchAsync(List<OpenSanctionsEntity> entities)
        {
            try
            {
                // Use upsert logic - update if exists, insert if new
                foreach (var entity in entities)
                {
                    var existing = await _context.OpenSanctionsEntities
                        .FirstOrDefaultAsync(e => e.Id == entity.Id);

                    if (existing != null)
                    {
                        // Update existing entity
                        existing.Name = entity.Name;
                        existing.Schema = entity.Schema;
                        existing.Aliases = entity.Aliases;
                        existing.BirthDate = entity.BirthDate;
                        existing.Countries = entity.Countries;
                        existing.Addresses = entity.Addresses;
                        existing.Identifiers = entity.Identifiers;
                        existing.Sanctions = entity.Sanctions;
                        existing.Datasets = entity.Datasets;
                        existing.Score = entity.Score;
                        existing.FirstSeen = entity.FirstSeen;
                        existing.LastSeen = entity.LastSeen;
                        existing.LastChange = entity.LastChange;
                        existing.UpdatedAtUtc = DateTime.UtcNow;
                    }
                    else
                    {
                        // Insert new entity
                        entity.CreatedAtUtc = DateTime.UtcNow;
                        entity.UpdatedAtUtc = DateTime.UtcNow;
                        _context.OpenSanctionsEntities.Add(entity);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving batch of {Count} entities", entities.Count);
                throw;
            }
        }

        public async Task<List<OpenSanctionsEntity>> SearchEntitiesAsync(string query, int limit = 10)
        {
            try
            {
                var searchTerm = query.ToLowerInvariant();
                
                return await _context.OpenSanctionsEntities
                    .Where(e => e.Name.ToLower().Contains(searchTerm) || 
                               e.Aliases.ToLower().Contains(searchTerm))
                    .OrderByDescending(e => e.Score)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OpenSanctions entities for query: {Query}", query);
                return new List<OpenSanctionsEntity>();
            }
        }

        public async Task<OpenSanctionsEntity?> GetEntityByIdAsync(string entityId)
        {
            try
            {
                return await _context.OpenSanctionsEntities
                    .FirstOrDefaultAsync(e => e.Id == entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenSanctions entity by ID: {EntityId}", entityId);
                return null;
            }
        }

        public async Task<List<OpenSanctionsEntity>> MatchPersonAsync(string name, DateTime? dateOfBirth = null, string? nationality = null)
        {
            try
            {
                var query = _context.OpenSanctionsEntities
                    .Where(e => e.Schema == "Person");

                // Name matching
                var searchName = name.ToLowerInvariant();
                query = query.Where(e => e.Name.ToLower().Contains(searchName) || 
                                        e.Aliases.ToLower().Contains(searchName));

                // Date of birth matching
                if (dateOfBirth.HasValue)
                {
                    query = query.Where(e => e.BirthDate.HasValue && 
                                           e.BirthDate.Value.Date == dateOfBirth.Value.Date);
                }

                // Nationality matching
                if (!string.IsNullOrEmpty(nationality))
                {
                    var searchNationality = nationality.ToLowerInvariant();
                    query = query.Where(e => e.Countries.ToLower().Contains(searchNationality));
                }

                return await query
                    .OrderByDescending(e => e.Score)
                    .Take(50)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching person in OpenSanctions: {Name}", name);
                return new List<OpenSanctionsEntity>();
            }
        }

        public async Task<List<OpenSanctionsEntity>> MatchOrganizationAsync(string name, string? country = null)
        {
            try
            {
                var query = _context.OpenSanctionsEntities
                    .Where(e => e.Schema == "Organization" || e.Schema == "Company");

                // Name matching
                var searchName = name.ToLowerInvariant();
                query = query.Where(e => e.Name.ToLower().Contains(searchName) || 
                                        e.Aliases.ToLower().Contains(searchName));

                // Country matching
                if (!string.IsNullOrEmpty(country))
                {
                    var searchCountry = country.ToLowerInvariant();
                    query = query.Where(e => e.Countries.ToLower().Contains(searchCountry));
                }

                return await query
                    .OrderByDescending(e => e.Score)
                    .Take(50)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching organization in OpenSanctions: {Name}", name);
                return new List<OpenSanctionsEntity>();
            }
        }

        public async Task<DateTime?> GetLastUpdateTimeAsync()
        {
            try
            {
                var config = await _context.SystemConfigurations
                    .FirstOrDefaultAsync(c => c.Key == "OpenSanctionsLastUpdate");
                
                if (config != null && DateTime.TryParse(config.Value, out var lastUpdate))
                {
                    return lastUpdate;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenSanctions last update time");
                return null;
            }
        }

        private async Task UpdateLastUpdateTimeAsync()
        {
            try
            {
                var config = await _context.SystemConfigurations
                    .FirstOrDefaultAsync(c => c.Key == "OpenSanctionsLastUpdate");

                if (config != null)
                {
                    config.Value = DateTime.UtcNow.ToString("O");
                    config.UpdatedAtUtc = DateTime.UtcNow;
                }
                else
                {
                    config = new SystemConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Key = "OpenSanctionsLastUpdate",
                        Value = DateTime.UtcNow.ToString("O"),
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    _context.SystemConfigurations.Add(config);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OpenSanctions last update time");
            }
        }

        public async Task<int> GetTotalEntitiesCountAsync()
        {
            try
            {
                return await _context.OpenSanctionsEntities.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total OpenSanctions entities count");
                return 0;
            }
        }
    }
}
