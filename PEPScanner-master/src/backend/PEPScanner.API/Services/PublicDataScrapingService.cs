using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;
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
    private readonly IConfiguration _configuration;

    public PublicDataScrapingService(PepScannerDbContext context, HttpClient httpClient, ILogger<PublicDataScrapingService> logger, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<int> ScrapeRbiDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting RBI data scraping");
            var entries = new List<WatchlistEntry>();

            var urls = new[]
            {
                _configuration["ScrapingUrls:RBI:CautionList"],
                _configuration["ScrapingUrls:RBI:DefaultersList"]
            };

            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var html = await _httpClient.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var tables = doc.DocumentNode.SelectNodes("//table");
                    if (tables != null)
                    {
                        foreach (var table in tables)
                        {
                            var rows = table.SelectNodes(".//tr");
                            if (rows != null)
                            {
                                foreach (var row in rows.Skip(1))
                                {
                                    var cells = row.SelectNodes(".//td");
                                    if (cells?.Count >= 2)
                                    {
                                        var name = cells[0].InnerText?.Trim();
                                        var details = cells[1].InnerText?.Trim();
                                        
                                        if (!string.IsNullOrEmpty(name) && name.Length > 3)
                                        {
                                            entries.Add(new WatchlistEntry
                                            {
                                                Id = Guid.NewGuid(),
                                                ExternalId = $"RBI_{Guid.NewGuid().ToString("N")[..8]}",
                                                Source = "RBI",
                                                ListType = url.Contains("Caution") ? "Caution List" : "Defaulters List",
                                                PrimaryName = name,
                                                Country = "India",
                                                SanctionReason = details ?? "RBI Listed Entity",
                                                RiskCategory = "High",
                                                IsActive = true,
                                                DateAddedUtc = DateTime.UtcNow,
                                                DateLastUpdatedUtc = DateTime.UtcNow
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to scrape RBI URL: {Url}", url);
                }
            }

            await SaveEntries("RBI", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping RBI data");
            return 0;
        }
    }

    public async Task<int> ScrapeSebiDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting SEBI data scraping");
            var entries = new List<WatchlistEntry>();

            var urls = new[]
            {
                _configuration["ScrapingUrls:SEBI:EnforcementOrders"],
                _configuration["ScrapingUrls:SEBI:DebarredEntities"]
            };

            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var html = await _httpClient.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var links = doc.DocumentNode.SelectNodes("//a[contains(@href, 'order') or contains(text(), 'order') or contains(text(), 'debarred')]");
                    if (links != null)
                    {
                        foreach (var link in links.Take(20))
                        {
                            var text = link.InnerText?.Trim();
                            if (!string.IsNullOrEmpty(text) && text.Length > 10)
                            {
                                entries.Add(new WatchlistEntry
                                {
                                    Id = Guid.NewGuid(),
                                    ExternalId = $"SEBI_{Guid.NewGuid().ToString("N")[..8]}",
                                    Source = "SEBI",
                                    ListType = url.Contains("enforcement") ? "Enforcement Orders" : "Debarred Entities",
                                    PrimaryName = text.Length > 100 ? text[..100] : text,
                                    Country = "India",
                                    SanctionReason = "SEBI enforcement action",
                                    RiskCategory = "Medium",
                                    IsActive = true,
                                    DateAddedUtc = DateTime.UtcNow,
                                    DateLastUpdatedUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to scrape SEBI URL: {Url}", url);
                }
            }

            await SaveEntries("SEBI", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping SEBI data");
            return 0;
        }
    }

    public async Task<int> ScrapeParliamentDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting Parliament data scraping");
            var entries = new List<WatchlistEntry>();

            var urls = new[]
            {
                _configuration["ScrapingUrls:Parliament:LokSabhaMembers"],
                _configuration["ScrapingUrls:Parliament:RajyaSabhaMembers"]
            };

            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var html = await _httpClient.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var memberLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, 'member') or contains(@href, 'Member')]");
                    if (memberLinks != null)
                    {
                        foreach (var link in memberLinks.Take(50))
                        {
                            var name = link.InnerText?.Trim();
                            if (!string.IsNullOrEmpty(name) && name.Length > 3)
                            {
                                entries.Add(new WatchlistEntry
                                {
                                    Id = Guid.NewGuid(),
                                    ExternalId = $"PARL_{Guid.NewGuid().ToString("N")[..8]}",
                                    Source = "PARLIAMENT",
                                    ListType = "PEP",
                                    PrimaryName = name,
                                    Country = "India",
                                    PositionOrRole = url.Contains("loksabha") ? "Lok Sabha Member" : "Rajya Sabha Member",
                                    PepCategory = "Member of Parliament",
                                    PepPosition = url.Contains("loksabha") ? "MP (Lok Sabha)" : "MP (Rajya Sabha)",
                                    PepCountry = "India",
                                    RiskCategory = "Medium",
                                    IsActive = true,
                                    DateAddedUtc = DateTime.UtcNow,
                                    DateLastUpdatedUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to scrape Parliament URL: {Url}", url);
                }
            }

            await SaveEntries("PARLIAMENT", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Parliament data");
            return 0;
        }
    }

    public async Task<int> ScrapeWikipediaPepsAsync()
    {
        try
        {
            _logger.LogInformation("Starting Wikipedia PEP scraping");
            var entries = new List<WatchlistEntry>();

            var urls = new[]
            {
                _configuration["ScrapingUrls:Wikipedia:ChiefMinisters"],
                _configuration["ScrapingUrls:Wikipedia:Governors"],
                _configuration["ScrapingUrls:Wikipedia:CabinetMinisters"]
            };

            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                try
                {
                    var response = await _httpClient.GetStringAsync(url);
                    var data = JsonSerializer.Deserialize<JsonElement>(response);
                    
                    if (data.TryGetProperty("extract", out var extract))
                    {
                        var text = extract.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            var names = ExtractNamesFromText(text);
                            foreach (var name in names.Take(10))
                            {
                                var position = url.Contains("chief") ? "Chief Minister" : 
                                             url.Contains("governor") ? "Governor" : "Cabinet Minister";
                                
                                entries.Add(new WatchlistEntry
                                {
                                    Id = Guid.NewGuid(),
                                    ExternalId = $"WIKI_{Guid.NewGuid().ToString("N")[..8]}",
                                    Source = "WIKIPEDIA",
                                    ListType = "PEP",
                                    PrimaryName = name,
                                    Country = "India",
                                    PositionOrRole = position,
                                    PepCategory = "Government Official",
                                    PepPosition = position,
                                    PepCountry = "India",
                                    RiskCategory = "High",
                                    IsActive = true,
                                    DateAddedUtc = DateTime.UtcNow,
                                    DateLastUpdatedUtc = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to scrape Wikipedia URL: {Url}", url);
                }
            }

            await SaveEntries("WIKIPEDIA", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping Wikipedia data");
            return 0;
        }
    }

    public async Task<int> ScrapeOpenSanctionsAsync()
    {
        try
        {
            _logger.LogInformation("Starting OpenSanctions scraping");
            var entries = new List<WatchlistEntry>();

            var baseUrl = _configuration["ScrapingUrls:OpenSanctions:SearchApi"];
            var queries = new[]
            {
                _configuration["ScrapingUrls:OpenSanctions:IndiaQuery"],
                _configuration["ScrapingUrls:OpenSanctions:TerrorismQuery"]
            };

            foreach (var query in queries.Where(q => !string.IsNullOrEmpty(q)))
            {
                try
                {
                    var url = baseUrl + query;
                    var response = await _httpClient.GetStringAsync(url);
                    var data = JsonSerializer.Deserialize<JsonElement>(response);

                    if (data.TryGetProperty("results", out var results))
                    {
                        foreach (var result in results.EnumerateArray())
                        {
                            if (result.TryGetProperty("caption", out var caption))
                            {
                                var name = caption.GetString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    var schema = result.TryGetProperty("schema", out var schemaElement) ? schemaElement.GetString() : "Person";
                                    var country = result.TryGetProperty("countries", out var countriesElement) ? 
                                        string.Join(", ", countriesElement.EnumerateArray().Select(c => c.GetString())) : "Unknown";

                                    entries.Add(new WatchlistEntry
                                    {
                                        Id = Guid.NewGuid(),
                                        ExternalId = $"OS_{Guid.NewGuid().ToString("N")[..8]}",
                                        Source = "OPENSANCTIONS",
                                        ListType = "Sanctions",
                                        PrimaryName = name,
                                        Country = country,
                                        PositionOrRole = schema,
                                        RiskCategory = "High",
                                        SanctionReason = "Listed in OpenSanctions database",
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
                    _logger.LogWarning(ex, "Failed to scrape OpenSanctions query: {Query}", query);
                }
            }

            await SaveEntries("OPENSANCTIONS", entries);
            return entries.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping OpenSanctions data");
            return 0;
        }
    }

    private List<string> ExtractNamesFromText(string text)
    {
        var names = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length - 1; i++)
        {
            if (char.IsUpper(words[i][0]) && char.IsUpper(words[i + 1][0]))
            {
                var name = $"{words[i]} {words[i + 1]}";
                if (name.Length > 5 && !names.Contains(name))
                {
                    names.Add(name);
                }
            }
        }
        
        return names;
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
            _logger.LogError(ex, "Error saving entries for source {Source}", source);
            throw;
        }
    }
}