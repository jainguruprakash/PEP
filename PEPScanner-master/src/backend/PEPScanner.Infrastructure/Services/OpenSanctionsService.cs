using System.Text.Json;
using System.Text.Json.Serialization;
using PEPScanner.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PEPScanner.Infrastructure.Services
{
    public interface IOpenSanctionsService
    {
        Task<OpenSanctionsMatchResult> MatchPersonAsync(string name, DateTime? dateOfBirth = null, string? nationality = null);
        Task<OpenSanctionsMatchResult> MatchOrganizationAsync(string name, string? country = null);
        Task<List<OpenSanctionsServiceEntity>> SearchAsync(string query, int limit = 10);
        Task<OpenSanctionsServiceEntity?> GetEntityAsync(string entityId);
    }

    public class OpenSanctionsService : IOpenSanctionsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenSanctionsService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.opensanctions.org";

        public OpenSanctionsService(HttpClient httpClient, ILogger<OpenSanctionsService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["OpenSanctions:ApiKey"] ?? "";

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PEPScanner/1.0");
        }

        public async Task<OpenSanctionsMatchResult> MatchPersonAsync(string name, DateTime? dateOfBirth = null, string? nationality = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new OpenSanctionsMatchResult
                {
                    Query = name,
                    TotalResults = 0,
                    Matches = new List<PEPScanner.Domain.Entities.OpenSanctionsEntity>(),
                    SearchedAt = DateTime.UtcNow,
                    Error = "OpenSanctions API key not configured"
                };
            }

            try
            {
                var queryParams = new List<string>
                {
                    $"schema=Person",
                    $"name={Uri.EscapeDataString(name)}"
                };

                if (dateOfBirth.HasValue)
                {
                    queryParams.Add($"birth_date={dateOfBirth.Value:yyyy-MM-dd}");
                }

                if (!string.IsNullOrEmpty(nationality))
                {
                    queryParams.Add($"country={Uri.EscapeDataString(nationality)}");
                }

                var url = $"{_baseUrl}/match/sanctions?{string.Join("&", queryParams)}";
                
                _logger.LogInformation("Matching person against OpenSanctions: {Name}", name);
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenSanctionsApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var serviceEntities = result?.Results?.Select(MapToServiceEntity).ToList() ?? new List<OpenSanctionsServiceEntity>();
                return new OpenSanctionsMatchResult
                {
                    Query = name,
                    TotalResults = result?.Results?.Count ?? 0,
                    Matches = serviceEntities.Select(ConvertToDomainEntity).ToList(),
                    SearchedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching person against OpenSanctions: {Name}", name);
                return new OpenSanctionsMatchResult
                {
                    Query = name,
                    TotalResults = 0,
                    Matches = new List<PEPScanner.Domain.Entities.OpenSanctionsEntity>(),
                    SearchedAt = DateTime.UtcNow,
                    Error = ex.Message
                };
            }
        }

        public async Task<OpenSanctionsMatchResult> MatchOrganizationAsync(string name, string? country = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new OpenSanctionsMatchResult
                {
                    Query = name,
                    TotalResults = 0,
                    Matches = new List<PEPScanner.Domain.Entities.OpenSanctionsEntity>(),
                    SearchedAt = DateTime.UtcNow,
                    Error = "OpenSanctions API key not configured"
                };
            }

            try
            {
                var queryParams = new List<string>
                {
                    $"schema=Organization",
                    $"name={Uri.EscapeDataString(name)}"
                };

                if (!string.IsNullOrEmpty(country))
                {
                    queryParams.Add($"country={Uri.EscapeDataString(country)}");
                }

                var url = $"{_baseUrl}/match/sanctions?{string.Join("&", queryParams)}";
                
                _logger.LogInformation("Matching organization against OpenSanctions: {Name}", name);
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenSanctionsApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var serviceEntities = result?.Results?.Select(MapToServiceEntity).ToList() ?? new List<OpenSanctionsServiceEntity>();
                return new OpenSanctionsMatchResult
                {
                    Query = name,
                    TotalResults = result?.Results?.Count ?? 0,
                    Matches = serviceEntities.Select(ConvertToDomainEntity).ToList(),
                    SearchedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching organization against OpenSanctions: {Name}", name);
                return new OpenSanctionsMatchResult
                {
                    Query = name,
                    TotalResults = 0,
                    Matches = new List<PEPScanner.Domain.Entities.OpenSanctionsEntity>(),
                    SearchedAt = DateTime.UtcNow,
                    Error = ex.Message
                };
            }
        }

        public async Task<List<OpenSanctionsServiceEntity>> SearchAsync(string query, int limit = 10)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new List<OpenSanctionsServiceEntity>();
            }

            try
            {
                var url = $"{_baseUrl}/search/sanctions?q={Uri.EscapeDataString(query)}&limit={limit}";
                
                _logger.LogInformation("Searching OpenSanctions: {Query}", query);
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenSanctionsApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Results?.Select(MapToServiceEntity).ToList() ?? new List<OpenSanctionsServiceEntity>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OpenSanctions: {Query}", query);
                return new List<OpenSanctionsServiceEntity>();
            }
        }

        public async Task<OpenSanctionsServiceEntity?> GetEntityAsync(string entityId)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return null;
            }

            try
            {
                var url = $"{_baseUrl}/entities/{entityId}";
                
                _logger.LogInformation("Getting OpenSanctions entity: {EntityId}", entityId);
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenSanctionsApiEntity>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result != null ? MapToServiceEntity(result) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenSanctions entity: {EntityId}", entityId);
                return null;
            }
        }

        private OpenSanctionsServiceEntity MapToServiceEntity(OpenSanctionsApiEntity apiEntity)
        {
            return new OpenSanctionsServiceEntity
            {
                Id = apiEntity.Id ?? "",
                Schema = apiEntity.Schema ?? "",
                Name = apiEntity.Caption ?? "",
                Aliases = apiEntity.Properties?.Name ?? new List<string>(),
                BirthDate = ParseDate(apiEntity.Properties?.BirthDate?.FirstOrDefault()),
                Countries = apiEntity.Properties?.Country ?? new List<string>(),
                Addresses = apiEntity.Properties?.Address ?? new List<string>(),
                Identifiers = apiEntity.Properties?.IdNumber ?? new List<string>(),
                Sanctions = apiEntity.Properties?.Sanctions ?? new List<string>(),
                Datasets = apiEntity.Datasets ?? new List<string>(),
                Score = apiEntity.Score ?? 0.0,
                FirstSeen = ParseDate(apiEntity.FirstSeen),
                LastSeen = ParseDate(apiEntity.LastSeen),
                LastChange = ParseDate(apiEntity.LastChange)
            };
        }

        private PEPScanner.Domain.Entities.OpenSanctionsEntity ConvertToDomainEntity(OpenSanctionsServiceEntity serviceEntity)
        {
            return new PEPScanner.Domain.Entities.OpenSanctionsEntity
            {
                Id = serviceEntity.Id,
                Schema = serviceEntity.Schema,
                Name = serviceEntity.Name,
                Aliases = JsonSerializer.Serialize(serviceEntity.Aliases),
                BirthDate = serviceEntity.BirthDate,
                Countries = JsonSerializer.Serialize(serviceEntity.Countries),
                Addresses = JsonSerializer.Serialize(serviceEntity.Addresses),
                Identifiers = JsonSerializer.Serialize(serviceEntity.Identifiers),
                Sanctions = JsonSerializer.Serialize(serviceEntity.Sanctions),
                Datasets = JsonSerializer.Serialize(serviceEntity.Datasets),
                Score = serviceEntity.Score,
                FirstSeen = serviceEntity.FirstSeen,
                LastSeen = serviceEntity.LastSeen,
                LastChange = serviceEntity.LastChange
            };
        }

        private DateTime? ParseDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            if (DateTime.TryParse(dateString, out var date))
                return date;

            return null;
        }
    }

    // DTOs for API responses
    public class OpenSanctionsApiResponse
    {
        [JsonPropertyName("results")]
        public List<OpenSanctionsApiEntity>? Results { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("query")]
        public string? Query { get; set; }
    }

    public class OpenSanctionsApiEntity
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("schema")]
        public string? Schema { get; set; }

        [JsonPropertyName("caption")]
        public string? Caption { get; set; }

        [JsonPropertyName("properties")]
        public OpenSanctionsProperties? Properties { get; set; }

        [JsonPropertyName("datasets")]
        public List<string>? Datasets { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("first_seen")]
        public string? FirstSeen { get; set; }

        [JsonPropertyName("last_seen")]
        public string? LastSeen { get; set; }

        [JsonPropertyName("last_change")]
        public string? LastChange { get; set; }
    }

    public class OpenSanctionsProperties
    {
        [JsonPropertyName("name")]
        public List<string>? Name { get; set; }

        [JsonPropertyName("birthDate")]
        public List<string>? BirthDate { get; set; }

        [JsonPropertyName("country")]
        public List<string>? Country { get; set; }

        [JsonPropertyName("address")]
        public List<string>? Address { get; set; }

        [JsonPropertyName("idNumber")]
        public List<string>? IdNumber { get; set; }

        [JsonPropertyName("sanctions")]
        public List<string>? Sanctions { get; set; }
    }

    // Service DTOs (different from domain entities - these have List<string> properties)
    public class OpenSanctionsServiceEntity
    {
        public string Id { get; set; } = "";
        public string Schema { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> Aliases { get; set; } = new();
        public DateTime? BirthDate { get; set; }
        public List<string> Countries { get; set; } = new();
        public List<string> Addresses { get; set; } = new();
        public List<string> Identifiers { get; set; } = new();
        public List<string> Sanctions { get; set; } = new();
        public List<string> Datasets { get; set; } = new();
        public double Score { get; set; }
        public DateTime? FirstSeen { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? LastChange { get; set; }
    }
}
