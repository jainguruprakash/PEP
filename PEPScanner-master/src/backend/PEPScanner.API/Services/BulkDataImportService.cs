using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;
using System.Xml.Linq;
using System.Globalization;
using CsvHelper;

namespace PEPScanner.API.Services;

public interface IBulkDataImportService
{
    Task<int> ImportOpenSanctionsBulkDataAsync();
    Task<int> ImportOfacBulkDataAsync();
    Task<int> ImportUnBulkDataAsync();
    Task<int> ImportEuBulkDataAsync();
    Task<int> ImportUkBulkDataAsync();
    Task<int> ImportWorldBankDataAsync();
    Task<int> ImportInterpolDataAsync();
    Task<int> ImportRiskProDataAsync();
    Task<int> ImportRegTechTimesDataAsync();
    Task<int> ImportGlobalVendorsDataAsync();
    Task<int> ImportAllBulkDataAsync();
}

public class BulkDataImportService : IBulkDataImportService
{
    private readonly PepScannerDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BulkDataImportService> _logger;
    private readonly IConfiguration _configuration;

    public BulkDataImportService(PepScannerDbContext context, HttpClient httpClient, ILogger<BulkDataImportService> logger, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _httpClient.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<int> ImportOpenSanctionsBulkDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting OpenSanctions bulk import");
            var entries = new List<WatchlistEntry>();

            var csvUrl = _configuration["BulkDataSources:OpenSanctions:CsvUrl"];
            var csvData = await _httpClient.GetStringAsync(csvUrl);

            using var reader = new StringReader(csvData);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<dynamic>().ToList();
            
            foreach (var record in records)
            {
                var recordDict = (IDictionary<string, object>)record;
                
                if (recordDict.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name?.ToString()))
                {
                    entries.Add(new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = recordDict.TryGetValue("id", out var id) ? id?.ToString() : Guid.NewGuid().ToString(),
                        Source = "OPENSANCTIONS",
                        ListType = recordDict.TryGetValue("dataset", out var dataset) ? dataset?.ToString() : "Sanctions",
                        PrimaryName = name.ToString(),
                        Country = recordDict.TryGetValue("countries", out var countries) ? countries?.ToString() : "Unknown",
                        SanctionReason = recordDict.TryGetValue("reason", out var reason) ? reason?.ToString() : "Sanctioned Entity",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    });
                }
            }

            await SaveBulkEntries("OPENSANCTIONS", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing OpenSanctions bulk data");
            return 0;
        }
    }

    public async Task<int> ImportOfacBulkDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting OFAC bulk import");
            var entries = new List<WatchlistEntry>();

            var xmlUrl = _configuration["BulkDataSources:OFAC:SdnListXml"];
            var xmlData = await _httpClient.GetStringAsync(xmlUrl);
            var doc = XDocument.Parse(xmlData);

            var sdnEntries = doc.Descendants("sdnEntry");
            foreach (var entry in sdnEntries)
            {
                var firstName = entry.Element("firstName")?.Value;
                var lastName = entry.Element("lastName")?.Value;
                var name = $"{firstName} {lastName}".Trim();
                
                if (!string.IsNullOrEmpty(name))
                {
                    entries.Add(new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = entry.Element("uid")?.Value ?? Guid.NewGuid().ToString(),
                        Source = "OFAC",
                        ListType = "SDN List",
                        PrimaryName = name,
                        Country = "USA",
                        SanctionReason = entry.Element("remarks")?.Value ?? "OFAC Sanctioned",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    });
                }
            }

            await SaveBulkEntries("OFAC", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing OFAC bulk data");
            return 0;
        }
    }

    public async Task<int> ImportUnBulkDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting UN bulk import");
            var entries = new List<WatchlistEntry>();

            var xmlUrl = _configuration["BulkDataSources:UN:ConsolidatedListXml"];
            var xmlData = await _httpClient.GetStringAsync(xmlUrl);
            var doc = XDocument.Parse(xmlData);

            var individuals = doc.Descendants("INDIVIDUAL");
            foreach (var individual in individuals)
            {
                var name = individual.Element("FIRST_NAME")?.Value + " " + individual.Element("SECOND_NAME")?.Value;
                name = name.Trim();
                
                if (!string.IsNullOrEmpty(name))
                {
                    entries.Add(new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = individual.Element("DATAID")?.Value ?? Guid.NewGuid().ToString(),
                        Source = "UN",
                        ListType = "UN Sanctions List",
                        PrimaryName = name,
                        Country = individual.Element("LIST_TYPE")?.Value ?? "Unknown",
                        SanctionReason = individual.Element("COMMENTS1")?.Value ?? "UN Sanctioned",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    });
                }
            }

            await SaveBulkEntries("UN", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing UN bulk data");
            return 0;
        }
    }

    public async Task<int> ImportEuBulkDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting EU bulk import");
            var entries = new List<WatchlistEntry>();

            var csvUrl = _configuration["BulkDataSources:EU:ConsolidatedListCsv"];
            var csvData = await _httpClient.GetStringAsync(csvUrl);

            using var reader = new StringReader(csvData);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<dynamic>().ToList();
            
            foreach (var record in records)
            {
                var recordDict = (IDictionary<string, object>)record;
                
                if (recordDict.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name?.ToString()))
                {
                    entries.Add(new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = recordDict.TryGetValue("id", out var id) ? id?.ToString() : Guid.NewGuid().ToString(),
                        Source = "EU",
                        ListType = "EU Sanctions List",
                        PrimaryName = name.ToString(),
                        Country = recordDict.TryGetValue("country", out var country) ? country?.ToString() : "EU",
                        SanctionReason = recordDict.TryGetValue("reason", out var reason) ? reason?.ToString() : "EU Sanctioned",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    });
                }
            }

            await SaveBulkEntries("EU", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing EU bulk data");
            return 0;
        }
    }

    public async Task<int> ImportUkBulkDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting UK bulk import");
            var entries = new List<WatchlistEntry>();

            var jsonUrl = _configuration["BulkDataSources:UK:ConsolidatedListJson"];
            var jsonData = await _httpClient.GetStringAsync(jsonUrl);
            var data = JsonSerializer.Deserialize<JsonElement>(jsonData);

            if (data.TryGetProperty("sanctions", out var sanctions))
            {
                foreach (var sanction in sanctions.EnumerateArray())
                {
                    if (sanction.TryGetProperty("name", out var nameElement))
                    {
                        var name = nameElement.GetString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            entries.Add(new WatchlistEntry
                            {
                                Id = Guid.NewGuid(),
                                ExternalId = sanction.TryGetProperty("id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                                Source = "UK",
                                ListType = "UK Sanctions List",
                                PrimaryName = name,
                                Country = sanction.TryGetProperty("country", out var country) ? country.GetString() : "UK",
                                SanctionReason = sanction.TryGetProperty("reason", out var reason) ? reason.GetString() : "UK Sanctioned",
                                RiskCategory = "High",
                                IsActive = true,
                                DateAddedUtc = DateTime.UtcNow,
                                DateLastUpdatedUtc = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            await SaveBulkEntries("UK", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing UK bulk data");
            return 0;
        }
    }

    public async Task<int> ImportWorldBankDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting World Bank bulk import");
            var entries = new List<WatchlistEntry>();

            var firmsUrl = _configuration["BulkDataSources:WorldBank:DebarredFirmsUrl"];
            var individualsUrl = _configuration["BulkDataSources:WorldBank:DebarredIndividualsUrl"];

            var urls = new[] { firmsUrl, individualsUrl };
            
            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var jsonData = await _httpClient.GetStringAsync(url);
                    var data = JsonSerializer.Deserialize<JsonElement>(jsonData);

                    if (data.TryGetProperty("results", out var results))
                    {
                        foreach (var result in results.EnumerateArray())
                        {
                            if (result.TryGetProperty("name", out var nameElement))
                            {
                                var name = nameElement.GetString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    entries.Add(new WatchlistEntry
                                    {
                                        Id = Guid.NewGuid(),
                                        ExternalId = result.TryGetProperty("id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                                        Source = "WORLDBANK",
                                        ListType = url.Contains("firms") ? "Debarred Firms" : "Debarred Individuals",
                                        PrimaryName = name,
                                        Country = result.TryGetProperty("country", out var country) ? country.GetString() : "Global",
                                        SanctionReason = "World Bank Debarred",
                                        RiskCategory = "Medium",
                                        IsActive = true,
                                        DateAddedUtc = DateTime.UtcNow,
                                        DateLastUpdatedUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import from World Bank URL: {Url}", url);
                }
            }

            await SaveBulkEntries("WORLDBANK", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing World Bank bulk data");
            return 0;
        }
    }

    public async Task<int> ImportInterpolDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting Interpol bulk import");
            var entries = new List<WatchlistEntry>();

            var redNoticesUrl = _configuration["BulkDataSources:Interpol:RedNoticesUrl"];
            var jsonData = await _httpClient.GetStringAsync(redNoticesUrl + "?resultPerPage=200");
            var data = JsonSerializer.Deserialize<JsonElement>(jsonData);

            if (data.TryGetProperty("_embedded", out var embedded) && 
                embedded.TryGetProperty("notices", out var notices))
            {
                foreach (var notice in notices.EnumerateArray())
                {
                    if (notice.TryGetProperty("forename", out var forename) && 
                        notice.TryGetProperty("name", out var surname))
                    {
                        var name = $"{forename.GetString()} {surname.GetString()}".Trim();
                        if (!string.IsNullOrEmpty(name))
                        {
                            entries.Add(new WatchlistEntry
                            {
                                Id = Guid.NewGuid(),
                                ExternalId = notice.TryGetProperty("entity_id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                                Source = "INTERPOL",
                                ListType = "Red Notices",
                                PrimaryName = name,
                                Country = notice.TryGetProperty("nationalities", out var nat) ? nat.ToString() : "Unknown",
                                SanctionReason = "Interpol Red Notice",
                                RiskCategory = "High",
                                IsActive = true,
                                DateAddedUtc = DateTime.UtcNow,
                                DateLastUpdatedUtc = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            await SaveBulkEntries("INTERPOL", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Interpol bulk data");
            return 0;
        }
    }

    public async Task<int> ImportRiskProDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting RiskPro bulk import");
            var entries = new List<WatchlistEntry>();

            var urls = new[]
            {
                _configuration["BulkDataSources:RiskPro:IndianPepUrl"],
                _configuration["BulkDataSources:RiskPro:GlobalPepUrl"],
                _configuration["BulkDataSources:RiskPro:SanctionsUrl"]
            };

            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var jsonData = await _httpClient.GetStringAsync(url);
                    var data = JsonSerializer.Deserialize<JsonElement>(jsonData);

                    if (data.TryGetProperty("data", out var dataArray))
                    {
                        foreach (var item in dataArray.EnumerateArray())
                        {
                            if (item.TryGetProperty("name", out var nameElement))
                            {
                                var name = nameElement.GetString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    entries.Add(new WatchlistEntry
                                    {
                                        Id = Guid.NewGuid(),
                                        ExternalId = item.TryGetProperty("id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                                        Source = "RISKPRO",
                                        ListType = url.Contains("pep") ? "PEP" : "Sanctions",
                                        PrimaryName = name,
                                        Country = item.TryGetProperty("country", out var country) ? country.GetString() : "India",
                                        PositionOrRole = item.TryGetProperty("position", out var position) ? position.GetString() : null,
                                        PepCategory = url.Contains("pep") ? "Politically Exposed Person" : null,
                                        RiskCategory = "Medium",
                                        IsActive = true,
                                        DateAddedUtc = DateTime.UtcNow,
                                        DateLastUpdatedUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import from RiskPro URL: {Url}", url);
                }
            }

            await SaveBulkEntries("RISKPRO", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing RiskPro bulk data");
            return 0;
        }
    }

    public async Task<int> ImportRegTechTimesDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting RegTech Times bulk import");
            var entries = new List<WatchlistEntry>();

            var urls = new[]
            {
                _configuration["BulkDataSources:RegTechTimes:PepDatabaseUrl"],
                _configuration["BulkDataSources:RegTechTimes:SanctionsDatabaseUrl"]
            };

            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var jsonData = await _httpClient.GetStringAsync(url);
                    var data = JsonSerializer.Deserialize<JsonElement>(jsonData);

                    if (data.TryGetProperty("records", out var records))
                    {
                        foreach (var record in records.EnumerateArray())
                        {
                            if (record.TryGetProperty("name", out var nameElement))
                            {
                                var name = nameElement.GetString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    entries.Add(new WatchlistEntry
                                    {
                                        Id = Guid.NewGuid(),
                                        ExternalId = record.TryGetProperty("id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                                        Source = "REGTECHTIMES",
                                        ListType = url.Contains("pep") ? "PEP Database" : "Sanctions Database",
                                        PrimaryName = name,
                                        Country = record.TryGetProperty("country", out var country) ? country.GetString() : "Global",
                                        PositionOrRole = record.TryGetProperty("role", out var role) ? role.GetString() : null,
                                        RiskCategory = "Medium",
                                        IsActive = true,
                                        DateAddedUtc = DateTime.UtcNow,
                                        DateLastUpdatedUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import from RegTech Times URL: {Url}", url);
                }
            }

            await SaveBulkEntries("REGTECHTIMES", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing RegTech Times bulk data");
            return 0;
        }
    }

    public async Task<int> ImportGlobalVendorsDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting Global Vendors bulk import");
            var entries = new List<WatchlistEntry>();

            var urls = new[]
            {
                _configuration["BulkDataSources:GlobalVendors:ComplianceDataUrl"],
                _configuration["BulkDataSources:GlobalVendors:PepDataUrl"],
                _configuration["BulkDataSources:GlobalVendors:SanctionsDataUrl"]
            };

            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var jsonData = await _httpClient.GetStringAsync(url);
                    var data = JsonSerializer.Deserialize<JsonElement>(jsonData);

                    if (data.TryGetProperty("entities", out var entities))
                    {
                        foreach (var entity in entities.EnumerateArray())
                        {
                            if (entity.TryGetProperty("name", out var nameElement))
                            {
                                var name = nameElement.GetString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    entries.Add(new WatchlistEntry
                                    {
                                        Id = Guid.NewGuid(),
                                        ExternalId = entity.TryGetProperty("id", out var id) ? id.GetString() : Guid.NewGuid().ToString(),
                                        Source = "GLOBALVENDORS",
                                        ListType = url.Contains("pep") ? "PEP Data" : url.Contains("sanctions") ? "Sanctions Data" : "Compliance Data",
                                        PrimaryName = name,
                                        Country = entity.TryGetProperty("country", out var country) ? country.GetString() : "Global",
                                        PositionOrRole = entity.TryGetProperty("position", out var position) ? position.GetString() : null,
                                        RiskCategory = "Medium",
                                        IsActive = true,
                                        DateAddedUtc = DateTime.UtcNow,
                                        DateLastUpdatedUtc = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import from Global Vendors URL: {Url}", url);
                }
            }

            await SaveBulkEntries("GLOBALVENDORS", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Global Vendors bulk data");
            return 0;
        }
    }

    public async Task<int> ImportAllBulkDataAsync()
    {
        _logger.LogInformation("Starting bulk import from all sources");
        
        var tasks = new[]
        {
            ImportOpenSanctionsBulkDataAsync(),
            ImportOfacBulkDataAsync(),
            ImportUnBulkDataAsync(),
            ImportEuBulkDataAsync(),
            ImportUkBulkDataAsync(),
            ImportWorldBankDataAsync(),
            ImportInterpolDataAsync(),
            ImportRiskProDataAsync(),
            ImportRegTechTimesDataAsync(),
            ImportGlobalVendorsDataAsync()
        };

        var results = await Task.WhenAll(tasks);
        var totalImported = results.Sum();
        
        _logger.LogInformation("Bulk import completed. Total records imported: {Total}", totalImported);
        return totalImported;
    }

    private async Task SaveBulkEntries(string source, List<WatchlistEntry> entries)
    {
        if (!entries.Any()) return;

        _logger.LogInformation("Bulk saving {Count} entries for source {Source}", entries.Count, source);

        // Remove existing entries for this source
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"WatchlistEntries\" WHERE \"Source\" = {0}", source);

        // Batch insert in chunks of 1000
        const int batchSize = 1000;
        for (int i = 0; i < entries.Count; i += batchSize)
        {
            var batch = entries.Skip(i).Take(batchSize).ToList();
            await _context.WatchlistEntries.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved batch {BatchNumber} for {Source} ({Count} records)", i / batchSize + 1, source, batch.Count);
        }

        _logger.LogInformation("Successfully bulk saved {Count} entries for {Source}", entries.Count, source);
    }
}