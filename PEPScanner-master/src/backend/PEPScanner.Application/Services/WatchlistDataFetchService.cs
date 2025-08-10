using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Application.Contracts;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;
using System.Text.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace PEPScanner.Application.Services
{
    public class WatchlistDataFetchService : IWatchlistDataFetchService
    {
        private readonly HttpClient _httpClient;
        private readonly PepScannerDbContext _context;
        private readonly ILogger<WatchlistDataFetchService> _logger;

        // Data source URLs
        private const string OFAC_SDN_URL = "https://www.treasury.gov/ofac/downloads/sdn.xml";
        private const string UN_SANCTIONS_URL = "https://scsanctions.un.org/resources/xml/en/consolidated.xml";
        private const string RBI_SANCTIONS_URL = "https://www.rbi.org.in/Scripts/BS_PressReleaseDisplay.aspx?prid=55126";
        private const string EU_SANCTIONS_URL = "https://webgate.ec.europa.eu/europeaid/fsd/fsf/public/files/xmlFullSanctionsList_1_1/content";
        private const string UK_SANCTIONS_URL = "https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/1017142/UK_Sanctions_List.csv";

        public WatchlistDataFetchService(
            HttpClient httpClient,
            PepScannerDbContext context,
            ILogger<WatchlistDataFetchService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
        }

        public async Task<WatchlistUpdateResult> FetchOfacDataAsync(CancellationToken cancellationToken = default)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "OFAC",
                ProcessingDate = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting OFAC data fetch from {Url}", OFAC_SDN_URL);

                var response = await _httpClient.GetStringAsync(OFAC_SDN_URL, cancellationToken);
                var xmlDoc = XDocument.Parse(response);

                var entries = new List<WatchlistEntry>();
                var sdnEntries = xmlDoc.Descendants("sdnEntry");

                foreach (var sdnEntry in sdnEntries)
                {
                    var uid = sdnEntry.Attribute("uid")?.Value;
                    var firstName = sdnEntry.Element("firstName")?.Value ?? "";
                    var lastName = sdnEntry.Element("lastName")?.Value ?? "";
                    var fullName = $"{firstName} {lastName}".Trim();
                    
                    if (string.IsNullOrEmpty(fullName))
                    {
                        fullName = sdnEntry.Element("title")?.Value ?? "Unknown";
                    }

                    var entry = new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = uid ?? Guid.NewGuid().ToString(),
                        Source = "OFAC",
                        ListType = "Sanctions",
                        PrimaryName = fullName,
                        AlternateNames = GetAlternateNames(sdnEntry),
                        DateOfBirth = ParseDate(sdnEntry.Element("dateOfBirth")?.Value),
                        PlaceOfBirth = sdnEntry.Element("placeOfBirth")?.Value,
                        Country = GetCountry(sdnEntry),
                        PositionOrRole = sdnEntry.Element("title")?.Value,
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        LastUpdatedUtc = DateTime.UtcNow
                    };

                    entries.Add(entry);
                }

                // Update database
                await UpdateWatchlistEntriesAsync("OFAC", entries);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count; // Simplified for now
                
                _logger.LogInformation("Successfully processed {Count} OFAC entries", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching OFAC data");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public async Task<WatchlistUpdateResult> FetchUnSanctionsDataAsync(CancellationToken cancellationToken = default)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "UN",
                ProcessingDate = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting UN Sanctions data fetch from {Url}", UN_SANCTIONS_URL);

                var response = await _httpClient.GetStringAsync(UN_SANCTIONS_URL, cancellationToken);
                var xmlDoc = XDocument.Parse(response);

                var entries = new List<WatchlistEntry>();
                var individuals = xmlDoc.Descendants("INDIVIDUAL");

                foreach (var individual in individuals)
                {
                    var dataid = individual.Attribute("DATAID")?.Value;
                    var firstName = individual.Element("FIRST_NAME")?.Value ?? "";
                    var secondName = individual.Element("SECOND_NAME")?.Value ?? "";
                    var thirdName = individual.Element("THIRD_NAME")?.Value ?? "";
                    var fourthName = individual.Element("FOURTH_NAME")?.Value ?? "";
                    
                    var fullName = $"{firstName} {secondName} {thirdName} {fourthName}".Trim();
                    fullName = Regex.Replace(fullName, @"\s+", " ");

                    var entry = new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = dataid ?? Guid.NewGuid().ToString(),
                        Source = "UN",
                        ListType = "Sanctions",
                        PrimaryName = fullName,
                        AlternateNames = GetUnAlternateNames(individual),
                        DateOfBirth = ParseUnDate(individual.Element("INDIVIDUAL_DATE_OF_BIRTH")),
                        PlaceOfBirth = individual.Element("INDIVIDUAL_PLACE_OF_BIRTH")?.Value,
                        Country = GetUnCountry(individual),
                        PositionOrRole = individual.Element("DESIGNATION")?.Value,
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        LastUpdatedUtc = DateTime.UtcNow
                    };

                    entries.Add(entry);
                }

                // Update database
                await UpdateWatchlistEntriesAsync("UN", entries);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;
                
                _logger.LogInformation("Successfully processed {Count} UN entries", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching UN data");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public async Task<WatchlistUpdateResult> FetchRbiDataAsync(CancellationToken cancellationToken = default)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "RBI",
                ProcessingDate = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting RBI data fetch");

                // RBI data is typically in PDF format, so we'll create sample data for demonstration
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "RBI_001",
                        Source = "RBI",
                        ListType = "Sanctions",
                        PrimaryName = "Sample RBI Entry",
                        Country = "India",
                        RiskCategory = "Medium",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        LastUpdatedUtc = DateTime.UtcNow
                    }
                };

                await UpdateWatchlistEntriesAsync("RBI", entries);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;
                
                _logger.LogInformation("Successfully processed {Count} RBI entries", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching RBI data");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public async Task<WatchlistUpdateResult> FetchSebiDataAsync(CancellationToken cancellationToken = default)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "SEBI",
                ProcessingDate = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting SEBI data fetch");

                // SEBI data - sample implementation
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "SEBI_001",
                        Source = "SEBI",
                        ListType = "Regulatory",
                        PrimaryName = "Sample SEBI Entry",
                        Country = "India",
                        RiskCategory = "Medium",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        LastUpdatedUtc = DateTime.UtcNow
                    }
                };

                await UpdateWatchlistEntriesAsync("SEBI", entries);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;
                
                _logger.LogInformation("Successfully processed {Count} SEBI entries", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SEBI data");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public async Task<WatchlistUpdateResult> FetchEuSanctionsDataAsync(CancellationToken cancellationToken = default)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "EU",
                ProcessingDate = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting EU Sanctions data fetch");

                // EU Sanctions - sample implementation
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "EU_001",
                        Source = "EU",
                        ListType = "Sanctions",
                        PrimaryName = "Sample EU Sanctions Entry",
                        Country = "European Union",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        LastUpdatedUtc = DateTime.UtcNow
                    }
                };

                await UpdateWatchlistEntriesAsync("EU", entries);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;
                
                _logger.LogInformation("Successfully processed {Count} EU entries", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching EU data");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public async Task<WatchlistUpdateResult> FetchUkSanctionsDataAsync(CancellationToken cancellationToken = default)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "UK",
                ProcessingDate = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting UK Sanctions data fetch");

                // UK Sanctions - sample implementation
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "UK_001",
                        Source = "UK",
                        ListType = "Sanctions",
                        PrimaryName = "Sample UK Sanctions Entry",
                        Country = "United Kingdom",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        LastUpdatedUtc = DateTime.UtcNow
                    }
                };

                await UpdateWatchlistEntriesAsync("UK", entries);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;
                
                _logger.LogInformation("Successfully processed {Count} UK entries", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching UK data");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public async Task<WatchlistUpdateResult> FetchIndianParliamentDataAsync(CancellationToken cancellationToken = default)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "PARLIAMENT",
                ProcessingDate = DateTime.UtcNow
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting Indian Parliament PEP data fetch");

                // Indian Parliament PEP data - sample implementation
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "PARL_001",
                        Source = "PARLIAMENT",
                        ListType = "PEP",
                        PrimaryName = "Sample Parliament Member",
                        Country = "India",
                        PositionOrRole = "Member of Parliament",
                        RiskCategory = "Medium",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        LastUpdatedUtc = DateTime.UtcNow
                    }
                };

                await UpdateWatchlistEntriesAsync("PARLIAMENT", entries);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;
                
                _logger.LogInformation("Successfully processed {Count} Parliament entries", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Parliament data");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public async Task<List<WatchlistUpdateResult>> FetchAllWatchlistDataAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<WatchlistUpdateResult>();

            _logger.LogInformation("Starting fetch for all watchlist sources");

            var tasks = new List<Task<WatchlistUpdateResult>>
            {
                FetchOfacDataAsync(cancellationToken),
                FetchUnSanctionsDataAsync(cancellationToken),
                FetchRbiDataAsync(cancellationToken),
                FetchSebiDataAsync(cancellationToken),
                FetchEuSanctionsDataAsync(cancellationToken),
                FetchUkSanctionsDataAsync(cancellationToken),
                FetchIndianParliamentDataAsync(cancellationToken)
            };

            try
            {
                var completedResults = await Task.WhenAll(tasks);
                results.AddRange(completedResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch watchlist fetch");
            }

            _logger.LogInformation("Completed fetch for all watchlist sources. {SuccessCount}/{TotalCount} successful",
                results.Count(r => r.Success), results.Count);

            return results;
        }

        public async Task<DateTime?> GetLastUpdateAsync(string source)
        {
            return await _context.WatchlistEntries
                .Where(w => w.Source == source)
                .MaxAsync(w => (DateTime?)w.LastUpdatedUtc);
        }

        public async Task<bool> ShouldUpdateSourceAsync(string source)
        {
            var lastUpdate = await GetLastUpdateAsync(source);
            
            if (lastUpdate == null)
                return true; // Never updated, should update

            // Default update frequency: daily
            var updateFrequency = TimeSpan.FromDays(1);
            
            // Different frequencies for different sources
            switch (source.ToUpper())
            {
                case "OFAC":
                case "UN":
                    updateFrequency = TimeSpan.FromHours(12); // Twice daily
                    break;
                case "RBI":
                case "SEBI":
                    updateFrequency = TimeSpan.FromDays(7); // Weekly
                    break;
                case "PARLIAMENT":
                    updateFrequency = TimeSpan.FromDays(30); // Monthly
                    break;
            }

            return DateTime.UtcNow - lastUpdate.Value > updateFrequency;
        }

        private async Task UpdateWatchlistEntriesAsync(string source, List<WatchlistEntry> newEntries)
        {
            // Mark existing entries as inactive
            var existingEntries = await _context.WatchlistEntries
                .Where(w => w.Source == source)
                .ToListAsync();

            foreach (var existing in existingEntries)
            {
                existing.IsActive = false;
                existing.LastUpdatedUtc = DateTime.UtcNow;
            }

            // Add new entries
            await _context.WatchlistEntries.AddRangeAsync(newEntries);
            await _context.SaveChangesAsync();
        }

        private string GetAlternateNames(XElement sdnEntry)
        {
            var aliases = sdnEntry.Descendants("alias")
                .Select(a => a.Attribute("aliasName")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            return aliases.Any() ? string.Join("; ", aliases) : null;
        }

        private string GetUnAlternateNames(XElement individual)
        {
            var aliases = individual.Descendants("INDIVIDUAL_ALIAS")
                .Select(a => a.Attribute("ALIAS_NAME")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            return aliases.Any() ? string.Join("; ", aliases) : null;
        }

        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return null;

            if (DateTime.TryParse(dateStr, out var date))
                return date;

            return null;
        }

        private DateTime? ParseUnDate(XElement dateElement)
        {
            if (dateElement == null)
                return null;

            var dateStr = dateElement.Value;
            return ParseDate(dateStr);
        }

        private string GetCountry(XElement sdnEntry)
        {
            return sdnEntry.Element("country")?.Value ?? 
                   sdnEntry.Descendants("address")
                           .FirstOrDefault()?.Element("country")?.Value;
        }

        private string GetUnCountry(XElement individual)
        {
            return individual.Element("INDIVIDUAL_PLACE_OF_BIRTH")?.Value ??
                   individual.Descendants("INDIVIDUAL_ADDRESS")
                            .FirstOrDefault()?.Element("COUNTRY")?.Value;
        }
    }
}
