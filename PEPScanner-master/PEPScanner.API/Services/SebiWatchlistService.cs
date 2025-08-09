using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Domain.Entities;
using CsvHelper;
using System.Globalization;
using System.Text.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace PEPScanner.API.Services
{
    /// <summary>
    /// SEBI (Securities and Exchange Board of India) Watchlist Service
    /// Handles SEBI debarred entities, defaulters, and other regulatory lists
    /// </summary>
    public class SebiWatchlistService : BaseWatchlistService
    {
        // SEBI configuration
        private readonly string _sebiBaseUrl;
        private readonly string _sebiDebarredEntities;
        private readonly string _sebiDefaulters;
        private readonly string _sebiSuspendedEntities;
        private readonly string _sebiPenalties;
        private readonly string _userAgent;
        private readonly int _requestTimeout;
        private readonly int _maxRetries;

        public override string SourceName => "SEBI";
        public override string DisplayName => "Securities and Exchange Board of India";
        public override string WatchlistType => "Local";
        public override string Country => "India";

        public SebiWatchlistService(
            PepScannerDbContext context,
            ILogger<SebiWatchlistService> logger,
            IConfiguration configuration,
            HttpClient httpClient) : base(context, logger, configuration, httpClient)
        {
            // Load SEBI configuration from appsettings.json
            _sebiBaseUrl = _configuration["SebiScraping:BaseUrl"] ?? "https://www.sebi.gov.in";
            _sebiDebarredEntities = _configuration["SebiScraping:DebarredEntitiesUrl"] ?? "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=1&smid=1";
            _sebiDefaulters = _configuration["SebiScraping:DefaultersUrl"] ?? "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=2&smid=1";
            _sebiSuspendedEntities = _configuration["SebiScraping:SuspendedEntitiesUrl"] ?? "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=3&smid=1";
            _sebiPenalties = _configuration["SebiScraping:PenaltiesUrl"] ?? "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=4&smid=1";
            _userAgent = _configuration["SebiScraping:UserAgent"] ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
            _requestTimeout = int.Parse(_configuration["SebiScraping:RequestTimeoutSeconds"] ?? "30");
            _maxRetries = int.Parse(_configuration["SebiScraping:MaxRetries"] ?? "3");

            _httpClient.Timeout = TimeSpan.FromSeconds(_requestTimeout);
        }

        public override async Task<List<WatchlistEntry>> FetchWatchlistDataAsync()
        {
            try
            {
                _logger.LogInformation("Fetching SEBI watchlist data via web scraping");
                
                var allEntries = new List<WatchlistEntry>();
                
                // Configure HttpClient for scraping
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                
                // Scrape different SEBI lists
                var debarredEntities = await ScrapeSebiListAsync(_sebiDebarredEntities, "Debarred Entity");
                allEntries.AddRange(debarredEntities);
                
                var defaulters = await ScrapeSebiListAsync(_sebiDefaulters, "Defaulter");
                allEntries.AddRange(defaulters);
                
                var suspendedEntities = await ScrapeSebiListAsync(_sebiSuspendedEntities, "Suspended Entity");
                allEntries.AddRange(suspendedEntities);
                
                var penalties = await ScrapeSebiListAsync(_sebiPenalties, "Penalty");
                allEntries.AddRange(penalties);
                
                _logger.LogInformation("Successfully scraped {Count} SEBI entries", allEntries.Count);
                
                return allEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SEBI watchlist data");
                throw;
            }
        }

        public override WatchlistSource GetSourceConfiguration()
        {
            return new WatchlistSource
            {
                Name = SourceName,
                DisplayName = DisplayName,
                Type = WatchlistType,
                Country = Country,
                Description = "SEBI regulatory lists including debarred entities, defaulters, suspended entities, and penalties",
                IsActive = true,
                UpdateFrequency = "Daily",
                DataFormat = "Web Scraping",
                WebScrapingUrl = _sebiBaseUrl
            };
        }

        /// <summary>
        /// Scrapes SEBI list data from the specified URL
        /// </summary>
        private async Task<List<WatchlistEntry>> ScrapeSebiListAsync(string url, string category)
        {
            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Scraping SEBI list: {Category} from {Url} (attempt {Attempt}/{MaxRetries})", 
                        category, url, attempt, _maxRetries);
                    
                    var response = await _httpClient.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(response);
                    
                    var entries = new List<WatchlistEntry>();
                    
                    // Look for table elements that contain the watchlist data
                    var tables = doc.DocumentNode.SelectNodes("//table[@class='table'] | //table[contains(@class, 'table')] | //table[@id='table']");
                    
                    if (tables == null || !tables.Any())
                    {
                        _logger.LogWarning("No tables found for category: {Category}", category);
                        return entries;
                    }
                    
                    foreach (var table in tables)
                    {
                        var rows = table.SelectNodes(".//tr");
                        if (rows == null) continue;
                        
                        // Skip header row
                        for (int i = 1; i < rows.Count; i++)
                        {
                            var row = rows[i];
                            var cells = row.SelectNodes(".//td");
                            
                            if (cells == null || cells.Count < 2) continue;
                            
                            var entry = ParseSebiTableRow(cells, category);
                            if (entry != null && !string.IsNullOrEmpty(entry.PrimaryName))
                            {
                                entries.Add(entry);
                            }
                        }
                    }
                    
                    _logger.LogInformation("Scraped {Count} entries for category: {Category}", entries.Count, category);
                    return entries;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scraping SEBI list for category: {Category} (attempt {Attempt}/{MaxRetries})", 
                        category, attempt, _maxRetries);
                    
                    if (attempt == _maxRetries)
                    {
                        _logger.LogError("Failed to scrape SEBI list for category: {Category} after {MaxRetries} attempts", 
                            category, _maxRetries);
                        return new List<WatchlistEntry>();
                    }
                    
                    // Wait before retrying
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
                }
            }
            
            return new List<WatchlistEntry>();
        }

        /// <summary>
        /// Parses SEBI table row data into a WatchlistEntry
        /// </summary>
        private WatchlistEntry? ParseSebiTableRow(HtmlNodeCollection cells, string category)
        {
            try
            {
                if (cells.Count < 2) return null;

                var entry = new WatchlistEntry
                {
                    Source = SourceName,
                    ListType = "Sanctions",
                    RiskCategory = category,
                    RiskLevel = GetRiskLevelForCategory(category),
                    EntityType = "Individual",
                    Country = "India",
                    DateAddedUtc = DateTime.UtcNow
                };

                // Parse based on cell count and content
                if (cells.Count >= 1)
                {
                    entry.PrimaryName = cells[0].InnerText.Trim();
                }

                if (cells.Count >= 2)
                {
                    var secondCell = cells[1].InnerText.Trim();
                    if (!string.IsNullOrEmpty(secondCell))
                    {
                        // Try to parse as date, if not, use as designation
                        if (DateTime.TryParse(secondCell, out var date))
                        {
                            entry.SanctionStartDate = date;
                        }
                        else
                        {
                            entry.PositionOrRole = secondCell;
                        }
                    }
                }

                if (cells.Count >= 3)
                {
                    var thirdCell = cells[2].InnerText.Trim();
                    if (!string.IsNullOrEmpty(thirdCell))
                    {
                        if (DateTime.TryParse(thirdCell, out var date))
                        {
                            entry.SanctionEndDate = date;
                        }
                        else
                        {
                            entry.SanctionReason = thirdCell;
                        }
                    }
                }

                if (cells.Count >= 4)
                {
                    entry.SanctionReason = cells[3].InnerText.Trim();
                }

                // Generate external ID if not available
                if (string.IsNullOrEmpty(entry.ExternalId))
                {
                    entry.ExternalId = $"SEBI_{category.Replace(" ", "_")}_{Guid.NewGuid().ToString("N")[..8]}";
                }

                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SEBI table row");
                return null;
            }
        }

        /// <summary>
        /// Determines risk level based on SEBI category
        /// </summary>
        private string GetRiskLevelForCategory(string category)
        {
            return category.ToLowerInvariant() switch
            {
                "debarred entity" => "High",
                "defaulter" => "High",
                "suspended entity" => "Medium",
                "penalty" => "Medium",
                _ => "Low"
            };
        }

        /// <summary>
        /// Override CSV parsing for SEBI-specific format
        /// </summary>
        protected override async Task<List<WatchlistEntry>> ParseCsvEntriesAsync(CsvReader csv)
        {
            var entries = new List<WatchlistEntry>();

            try
            {
                while (await csv.ReadAsync())
                {
                    var entry = new WatchlistEntry
                    {
                        Source = SourceName,
                        ListType = "Sanctions",
                        PrimaryName = csv.GetField("Name") ?? csv.GetField("EntityName") ?? csv.GetField("CompanyName") ?? "",
                        PositionOrRole = csv.GetField("Designation") ?? csv.GetField("Position") ?? "",
                        Country = "India",
                        EntityType = "Individual",
                        RiskCategory = csv.GetField("Category") ?? "SEBI",
                        RiskLevel = GetRiskLevelForCategory(csv.GetField("Category") ?? ""),
                        SanctionReason = csv.GetField("Reason") ?? csv.GetField("Violation") ?? "",
                        ExternalId = csv.GetField("SEBI_ID") ?? csv.GetField("ID") ?? "",
                        DateAddedUtc = DateTime.UtcNow
                    };

                    // Parse dates if available
                    if (DateTime.TryParse(csv.GetField("StartDate"), out var startDate))
                        entry.SanctionStartDate = startDate;

                    if (DateTime.TryParse(csv.GetField("EndDate"), out var endDate))
                        entry.SanctionEndDate = endDate;

                    if (!string.IsNullOrEmpty(entry.PrimaryName))
                    {
                        entries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SEBI CSV entries");
            }

            return entries;
        }

        /// <summary>
        /// Override Excel parsing for SEBI-specific format
        /// </summary>
        protected override async Task<List<WatchlistEntry>> ParseExcelEntriesAsync(IFormFile file)
        {
            var entries = new List<WatchlistEntry>();

            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    _logger.LogWarning("No worksheet found in SEBI Excel file");
                    return entries;
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2) return entries; // Need at least header + 1 data row

                // Find column indices
                var nameCol = FindColumnIndex(worksheet, "Name", "EntityName", "CompanyName");
                var designationCol = FindColumnIndex(worksheet, "Designation", "Position", "Role");
                var categoryCol = FindColumnIndex(worksheet, "Category", "Type");
                var reasonCol = FindColumnIndex(worksheet, "Reason", "Violation", "Description");
                var startDateCol = FindColumnIndex(worksheet, "StartDate", "FromDate");
                var endDateCol = FindColumnIndex(worksheet, "EndDate", "ToDate");
                var idCol = FindColumnIndex(worksheet, "SEBI_ID", "ID", "Reference");

                for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip header)
                {
                    var name = worksheet.Cells[row, nameCol].Value?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(name)) continue;

                    var entry = new WatchlistEntry
                    {
                        Source = SourceName,
                        ListType = "Sanctions",
                        PrimaryName = name,
                        PositionOrRole = worksheet.Cells[row, designationCol].Value?.ToString()?.Trim() ?? "",
                        Country = "India",
                        EntityType = "Individual",
                        RiskCategory = worksheet.Cells[row, categoryCol].Value?.ToString()?.Trim() ?? "SEBI",
                        RiskLevel = GetRiskLevelForCategory(worksheet.Cells[row, categoryCol].Value?.ToString() ?? ""),
                        SanctionReason = worksheet.Cells[row, reasonCol].Value?.ToString()?.Trim() ?? "",
                        ExternalId = worksheet.Cells[row, idCol].Value?.ToString()?.Trim() ?? "",
                        DateAddedUtc = DateTime.UtcNow
                    };

                    // Parse dates
                    if (DateTime.TryParse(worksheet.Cells[row, startDateCol].Value?.ToString(), out var startDate))
                        entry.SanctionStartDate = startDate;

                    if (DateTime.TryParse(worksheet.Cells[row, endDateCol].Value?.ToString(), out var endDate))
                        entry.SanctionEndDate = endDate;

                    entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SEBI Excel entries");
            }

            return entries;
        }

        /// <summary>
        /// Finds the column index for a given header name
        /// </summary>
        private int FindColumnIndex(ExcelWorksheet worksheet, params string[] possibleNames)
        {
            var colCount = worksheet.Dimension?.Columns ?? 0;
            for (int col = 1; col <= colCount; col++)
            {
                var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(headerValue))
                {
                    foreach (var name in possibleNames)
                    {
                        if (headerValue.Equals(name, StringComparison.OrdinalIgnoreCase))
                            return col;
                    }
                }
            }
            return 0; // Not found
        }

        /// <summary>
        /// Advanced SEBI scraping to find additional data sources
        /// </summary>
        public async Task<WatchlistUpdateResult> ScrapeSebiAdvancedAsync()
        {
            var result = new WatchlistUpdateResult
            {
                Source = SourceName,
                ProcessingDate = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting advanced SEBI scraping");

                // Scrape main SEBI page to find additional links
                var mainPageUrl = _sebiBaseUrl;
                var response = await _httpClient.GetStringAsync(mainPageUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // Look for links that might contain watchlist data
                var links = doc.DocumentNode.SelectNodes("//a[contains(@href, 'debar') or contains(@href, 'defaulter') or contains(@href, 'penalty') or contains(@href, 'suspension')]");
                
                if (links != null)
                {
                    foreach (var link in links.Take(10)) // Limit to first 10 links
                    {
                        var href = link.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            _logger.LogInformation("Found potential SEBI data link: {Link}", href);
                        }
                    }
                }

                result.Success = true;
                result.TotalRecords = 0; // This is just for discovery
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced SEBI scraping");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Search SEBI data by category
        /// </summary>
        public async Task<List<WatchlistEntry>> SearchSebiByCategoryAsync(string category)
        {
            try
            {
                var url = category.ToLowerInvariant() switch
                {
                    "debarred" or "debarred entity" => _sebiDebarredEntities,
                    "defaulter" or "defaulters" => _sebiDefaulters,
                    "suspended" or "suspended entity" => _sebiSuspendedEntities,
                    "penalty" or "penalties" => _sebiPenalties,
                    _ => _sebiDebarredEntities // Default
                };

                return await ScrapeSebiListAsync(url, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching SEBI data by category: {Category}", category);
                throw;
            }
        }
    }
}
