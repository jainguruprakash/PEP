using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Domain.Entities;
using PEPScanner.Application.Abstractions;
using System.Text.Json;

namespace PEPScanner.Infrastructure.Services
{
    public class UnSanctionsService : IUnSanctionsService
    {
        private readonly HttpClient _httpClient;
        private readonly PepScannerDbContext _context;
        private readonly ILogger<UnSanctionsService> _logger;

        // UN Sanctions API endpoints
        private const string UN_SANCTIONS_API = "https://scsanctions.un.org/resources/xml/en/consolidated.xml";
        private const string UN_SANCTIONS_CSV = "https://scsanctions.un.org/resources/csv/en/consolidated.csv";
        private const string UN_SANCTIONS_JSON = "https://scsanctions.un.org/resources/json/en/consolidated.json";

        public UnSanctionsService(
            HttpClient httpClient,
            PepScannerDbContext context,
            ILogger<UnSanctionsService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
        }

        public async Task<List<UnSanctionsEntry>> FetchUnSanctionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching UN sanctions data from {Url}", UN_SANCTIONS_JSON);
                
                var response = await _httpClient.GetAsync(UN_SANCTIONS_JSON);
                response.EnsureSuccessStatusCode();
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                return ParseUnSanctionsJson(jsonContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching UN sanctions data");
                throw;
            }
        }

        public async Task<WatchlistUpdateResult> UpdateWatchlistFromUnAsync()
        {
            var result = new WatchlistUpdateResult
            {
                Source = "UN",
                ProcessingDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Starting UN sanctions watchlist update");
                
                var unEntries = await FetchUnSanctionsAsync();
                result.TotalRecords = unEntries.Count;

                foreach (var unEntry in unEntries)
                {
                    if (string.IsNullOrEmpty(unEntry.Name))
                        continue;

                    var existingEntry = await _context.WatchlistEntries
                        .FirstOrDefaultAsync(w => w.ExternalId == unEntry.Id && w.Source == "UN");

                    if (existingEntry == null)
                    {
                        // Create new watchlist entry
                        var newEntry = new WatchlistEntry
                        {
                            Id = Guid.NewGuid(),
                            Source = "UN",
                            ListType = "Sanctions",
                            PrimaryName = unEntry.Name,
                            EntityType = GetEntityType(unEntry.Type),
                            RiskLevel = "High",
                            RiskCategory = "Sanctions",
                            Country = unEntry.Country,
                            Nationality = unEntry.Nationality,
                            Citizenship = unEntry.Nationality,
                            DateOfBirth = ParseDateOfBirth(unEntry.DateOfBirth),
                            PlaceOfBirth = unEntry.PlaceOfBirth,
                            Address = unEntry.Address,
                            PositionOrRole = unEntry.AdditionalInfo,
                            PepPosition = unEntry.AdditionalInfo,
                            PepCountry = unEntry.Country,
                            SanctionType = "Asset Freeze",
                            SanctionAuthority = "UN Security Council",
                            SanctionReference = unEntry.Id,
                            SanctionReason = unEntry.Comments,
                            ExternalId = unEntry.Id,
                            ExternalReference = unEntry.Id,
                            Comments = unEntry.Comments,
                            DateAddedUtc = DateTime.UtcNow,
                            IsActive = true,
                            AddedBy = "System"
                        };

                        _context.WatchlistEntries.Add(newEntry);
                        result.NewRecords++;
                    }
                    else
                    {
                        // Update existing entry
                        existingEntry.PrimaryName = unEntry.Name;
                        existingEntry.Country = unEntry.Country;
                        existingEntry.Nationality = unEntry.Nationality;
                        existingEntry.Citizenship = unEntry.Nationality;
                        existingEntry.DateOfBirth = ParseDateOfBirth(unEntry.DateOfBirth);
                        existingEntry.PlaceOfBirth = unEntry.PlaceOfBirth;
                        existingEntry.Address = unEntry.Address;
                        existingEntry.PositionOrRole = unEntry.AdditionalInfo;
                        existingEntry.Comments = unEntry.Comments;
                        existingEntry.DateLastUpdatedUtc = DateTime.UtcNow;
                        existingEntry.UpdatedBy = "System";

                        _context.WatchlistEntries.Update(existingEntry);
                        result.UpdatedRecords++;
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                
                _logger.LogInformation("UN sanctions watchlist update completed. {Result}", result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating watchlist from UN sanctions");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        public async Task<List<UnSanctionsEntry>> SearchByNameAsync(string name)
        {
            try
            {
                var allEntries = await FetchUnSanctionsAsync();
                return allEntries.Where(e => 
                    e.Name?.Contains(name, StringComparison.OrdinalIgnoreCase) == true ||
                    e.AdditionalInfo?.Contains(name, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching UN sanctions data for name: {Name}", name);
                throw;
            }
        }

        private List<UnSanctionsEntry> ParseUnSanctionsJson(string jsonContent)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var unData = JsonSerializer.Deserialize<UnSanctionsData>(jsonContent, options);
                return unData?.Entries ?? new List<UnSanctionsEntry>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing UN sanctions JSON content");
                throw;
            }
        }

        private string GetEntityType(string? type)
        {
            return type?.ToLower() switch
            {
                "individual" => "Individual",
                "entity" => "Organization",
                "vessel" => "Vessel",
                "aircraft" => "Aircraft",
                _ => "Individual"
            };
        }

        private DateTime? ParseDateOfBirth(string? dateOfBirth)
        {
            if (string.IsNullOrEmpty(dateOfBirth))
                return null;

            if (DateTime.TryParse(dateOfBirth, out var parsedDate))
                return parsedDate;

            return null;
        }
    }
}
