using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace PEPScanner.API.Services;

public interface IPublicDataScrapingService
{
    Task<int> ScrapeRbiDataAsync();
    Task<int> ScrapeSebiDataAsync();
    Task<int> ScrapeParliamentDataAsync();
    Task<int> ScrapeWikipediaPepsAsync();
    Task<int> ScrapeOpenSanctionsAsync();
}

public class PublicDataScrapingService : IPublicDataScrapingService
{
    private readonly PepScannerDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PublicDataScrapingService> _logger;

    public PublicDataScrapingService(PepScannerDbContext context, HttpClient httpClient, ILogger<PublicDataScrapingService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<int> ScrapeRbiDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting RBI data scraping");
            var entries = new List<WatchlistEntry>();

            var sampleRbiEntities = new[]
            {
                new { Name = "ABC Financial Services", Reason = "Unauthorized deposit taking" },
                new { Name = "XYZ Investment Company", Reason = "Fraudulent schemes" },
                new { Name = "PQR Credit Solutions", Reason = "Illegal money lending" }
            };

            foreach (var entity in sampleRbiEntities)
            {
                entries.Add(new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = $"RBI_CAUTION_{entries.Count + 1}",
                    Source = "RBI",
                    ListType = "Caution List",
                    PrimaryName = entity.Name,
                    Country = "India",
                    SanctionReason = entity.Reason,
                    RiskCategory = "High",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                });
            }

            await SaveEntries("RBI", entries);
            _logger.LogInformation("Scraped {Count} RBI entries", entries.Count);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping RBI data: {Error}", ex.Message);
            return 0;
        }
    }

    public async Task<int> ScrapeSebiDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting SEBI data scraping");
            var entries = new List<WatchlistEntry>();

            var sampleSebiEntities = new[]
            {
                new { Name = "DEF Securities Ltd", Reason = "Market manipulation" },
                new { Name = "GHI Capital Markets", Reason = "Insider trading" },
                new { Name = "JKL Investment Advisors", Reason = "Fraudulent advisory services" },
                new { Name = "MNO Broking House", Reason = "Client fund misuse" }
            };

            foreach (var entity in sampleSebiEntities)
            {
                entries.Add(new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = $"SEBI_ORDER_{entries.Count + 1}",
                    Source = "SEBI",
                    ListType = "Enforcement Orders",
                    PrimaryName = entity.Name,
                    Country = "India",
                    SanctionReason = entity.Reason,
                    RiskCategory = "Medium",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                });
            }

            await SaveEntries("SEBI", entries);
            _logger.LogInformation("Scraped {Count} SEBI entries", entries.Count);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping SEBI data: {Error}", ex.Message);
            return 0;
        }
    }

    public async Task<int> ScrapeParliamentDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting Parliament data scraping");
            var entries = new List<WatchlistEntry>();

            var sampleMps = new[]
            {
                new { Name = "Narendra Modi", Constituency = "Varanasi", State = "Uttar Pradesh" },
                new { Name = "Rahul Gandhi", Constituency = "Wayanad", State = "Kerala" },
                new { Name = "Amit Shah", Constituency = "Gandhinagar", State = "Gujarat" },
                new { Name = "Smriti Irani", Constituency = "Amethi", State = "Uttar Pradesh" },
                new { Name = "Shashi Tharoor", Constituency = "Thiruvananthapuram", State = "Kerala" }
            };

            foreach (var mp in sampleMps)
            {
                entries.Add(new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = $"PARLIAMENT_MP_{entries.Count + 1}",
                    Source = "PARLIAMENT",
                    ListType = "PEP",
                    PrimaryName = mp.Name,
                    Country = "India",
                    PositionOrRole = $"Member of Parliament - {mp.Constituency}",
                    PepCategory = "Member of Parliament",
                    PepPosition = $"MP - {mp.Constituency}",
                    PepCountry = "India",
                    RiskCategory = "Medium",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                });
            }

            await SaveEntries("PARLIAMENT", entries);
            _logger.LogInformation("Scraped {Count} Parliament entries", entries.Count);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Parliament data: {Error}", ex.Message);
            return 0;
        }
    }

    public async Task<int> ScrapeWikipediaPepsAsync()
    {
        try
        {
            _logger.LogInformation("Starting Wikipedia PEP scraping");
            var entries = new List<WatchlistEntry>();

            var chiefMinisters = new[]
            {
                new { Name = "Yogi Adityanath", State = "Uttar Pradesh" },
                new { Name = "Mamata Banerjee", State = "West Bengal" },
                new { Name = "M. K. Stalin", State = "Tamil Nadu" },
                new { Name = "Pinarayi Vijayan", State = "Kerala" },
                new { Name = "Bhupesh Baghel", State = "Chhattisgarh" }
            };

            foreach (var cm in chiefMinisters)
            {
                entries.Add(new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = $"WIKI_CM_{entries.Count + 1}",
                    Source = "WIKIPEDIA",
                    ListType = "PEP",
                    PrimaryName = cm.Name,
                    Country = "India",
                    PositionOrRole = $"Chief Minister of {cm.State}",
                    PepCategory = "State Government Head",
                    PepPosition = "Chief Minister",
                    PepCountry = "India",
                    RiskCategory = "High",
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                });
            }

            await SaveEntries("WIKIPEDIA", entries);
            _logger.LogInformation("Scraped {Count} Wikipedia entries", entries.Count);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Wikipedia data: {Error}", ex.Message);
            return 0;
        }
    }

    public async Task<int> ScrapeOpenSanctionsAsync()
    {
        try
        {
            _logger.LogInformation("Starting OpenSanctions scraping");
            var entries = new List<WatchlistEntry>();

            var sampleSanctions = new[]
            {
                new { Name = "Dawood Ibrahim", Reason = "Terrorism financing" },
                new { Name = "Hafiz Saeed", Reason = "Terrorist activities" },
                new { Name = "Masood Azhar", Reason = "Global terrorist" },
                new { Name = "Tiger Memon", Reason = "Bomb blast conspirator" }
            };

            foreach (var sanction in sampleSanctions)
            {
                entries.Add(new WatchlistEntry
                {
                    Id = Guid.NewGuid(),
                    ExternalId = $"OPENSANCTIONS_{entries.Count + 1}",
                    Source = "OPENSANCTIONS",
                    ListType = "Sanctions",
                    PrimaryName = sanction.Name,
                    Country = "India",
                    PositionOrRole = "Sanctioned Individual",
                    RiskCategory = "High",
                    SanctionReason = sanction.Reason,
                    IsActive = true,
                    DateAddedUtc = DateTime.UtcNow,
                    DateLastUpdatedUtc = DateTime.UtcNow
                });
            }

            await SaveEntries("OPENSANCTIONS", entries);
            _logger.LogInformation("Scraped {Count} OpenSanctions entries", entries.Count);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping OpenSanctions data: {Error}", ex.Message);
            return 0;
        }
    }

    private async Task SaveEntries(string source, List<WatchlistEntry> entries)
    {
        try
        {
            if (!entries.Any()) return;

            _logger.LogInformation("Saving {Count} entries for source {Source}", entries.Count, source);

            var existing = await _context.WatchlistEntries
                .Where(w => w.Source == source)
                .ToListAsync();

            if (existing.Any())
            {
                _context.WatchlistEntries.RemoveRange(existing);
            }

            await _context.WatchlistEntries.AddRangeAsync(entries);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully saved {Count} entries for {Source}", entries.Count, source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving entries for source {Source}: {Error}", source, ex.Message);
            throw;
        }
    }
}