using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Globalization;
using CsvHelper;

namespace PEPScanner.API.Services;

public interface IOpenSanctionsDatasetService
{
    Task<int> ImportCrimeDatasetAsync();
    Task<int> ImportSanctionsDatasetAsync();
    Task<int> ImportPepDatasetAsync();
    Task<int> ImportAllDatasetsAsync();
}

public class OpenSanctionsDatasetService : IOpenSanctionsDatasetService
{
    private readonly PepScannerDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenSanctionsDatasetService> _logger;

    public OpenSanctionsDatasetService(PepScannerDbContext context, HttpClient httpClient, ILogger<OpenSanctionsDatasetService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<int> ImportCrimeDatasetAsync()
    {
        var csvUrl = "https://data.opensanctions.org/datasets/latest/crime/targets.simple.csv";
        return await ImportDatasetAsync(csvUrl, "Crime", "OPENSANCTIONS_CRIME");
    }

    public async Task<int> ImportSanctionsDatasetAsync()
    {
        var csvUrl = "https://data.opensanctions.org/datasets/latest/sanctions/targets.simple.csv";
        return await ImportDatasetAsync(csvUrl, "Sanctions", "OPENSANCTIONS_SANCTIONS");
    }

    public async Task<int> ImportPepDatasetAsync()
    {
        var csvUrl = "https://data.opensanctions.org/datasets/latest/peps/targets.simple.csv";
        return await ImportDatasetAsync(csvUrl, "PEP", "OPENSANCTIONS_PEP");
    }

    public async Task<int> ImportAllDatasetsAsync()
    {
        var tasks = new[]
        {
            ImportCrimeDatasetAsync(),
            ImportSanctionsDatasetAsync(),
            ImportPepDatasetAsync()
        };

        var results = await Task.WhenAll(tasks);
        return results.Sum();
    }

    private async Task<int> ImportDatasetAsync(string csvUrl, string datasetType, string source)
    {
        try
        {
            _logger.LogInformation("Downloading {Type} dataset from {Url}", datasetType, csvUrl);
            var csvData = await _httpClient.GetStringAsync(csvUrl);

            using var reader = new StringReader(csvData);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<dynamic>().ToList();
            _logger.LogInformation("Processing {Count} records from {Type} dataset", records.Count, datasetType);

            var entries = new List<WatchlistEntry>();
            
            foreach (var record in records)
            {
                var recordDict = (IDictionary<string, object>)record;
                
                if (recordDict.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name?.ToString()))
                {
                    entries.Add(new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = recordDict.TryGetValue("id", out var id) ? id?.ToString() : Guid.NewGuid().ToString(),
                        Source = source,
                        ListType = datasetType,
                        PrimaryName = name.ToString(),
                        Country = recordDict.TryGetValue("countries", out var countries) ? countries?.ToString() : "Unknown",
                        SanctionReason = recordDict.TryGetValue("reason", out var reason) ? reason?.ToString() : $"OpenSanctions {datasetType}",
                        RiskCategory = datasetType == "Crime" ? "High" : datasetType == "PEP" ? "Medium" : "High",
                        PepCategory = datasetType == "PEP" ? "Politically Exposed Person" : null,
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    });
                }
            }

            await SaveBulkEntries(source, entries);
            _logger.LogInformation("Imported {Count} records from {Type} dataset", entries.Count, datasetType);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing {Type} dataset", datasetType);
            return 0;
        }
    }

    private async Task SaveBulkEntries(string source, List<WatchlistEntry> entries)
    {
        if (!entries.Any()) return;

        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"WatchlistEntries\" WHERE \"Source\" = {0}", source);

        const int batchSize = 1000;
        for (int i = 0; i < entries.Count; i += batchSize)
        {
            var batch = entries.Skip(i).Take(batchSize).ToList();
            await _context.WatchlistEntries.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
        }
    }
}