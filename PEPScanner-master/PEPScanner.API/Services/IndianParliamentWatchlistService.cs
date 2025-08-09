using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.API.Models;
using CsvHelper;
using System.Globalization;
using System.Text.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace PEPScanner.API.Services
{
    /// <summary>
    /// Indian Parliament Watchlist Service
    /// Handles current and former Members of Parliament (Lok Sabha and Rajya Sabha)
    /// </summary>
    public class IndianParliamentWatchlistService : BaseWatchlistService
    {
        // Parliament configuration
        private readonly string _parliamentBaseUrl;
        private readonly string _lokSabhaMembers;
        private readonly string _rajyaSabhaMembers;
        private readonly string _parliamentMinisters;
        private readonly string _parliamentCommittees;
        private readonly string _userAgent;
        private readonly int _requestTimeout;
        private readonly int _maxRetries;

        public override string SourceName => "IndianParliament";
        public override string DisplayName => "Indian Parliament Members";
        public override string WatchlistType => "Local";
        public override string Country => "India";

        public IndianParliamentWatchlistService(
            PepScannerDbContext context,
            ILogger<IndianParliamentWatchlistService> logger,
            IConfiguration configuration,
            HttpClient httpClient) : base(context, logger, configuration, httpClient)
        {
            // Load Parliament configuration from appsettings.json
            _parliamentBaseUrl = _configuration["ParliamentScraping:BaseUrl"] ?? "https://sansad.in";
            _lokSabhaMembers = _configuration["ParliamentScraping:LokSabhaMembersUrl"] ?? "https://sansad.in/ls/members";
            _rajyaSabhaMembers = _configuration["ParliamentScraping:RajyaSabhaMembersUrl"] ?? "https://sansad.in/rs/members";
            _parliamentMinisters = _configuration["ParliamentScraping:MinistersUrl"] ?? "https://sansad.in/ministers";
            _parliamentCommittees = _configuration["ParliamentScraping:CommitteesUrl"] ?? "https://sansad.in/committees";
            _userAgent = _configuration["ParliamentScraping:UserAgent"] ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
            _requestTimeout = int.Parse(_configuration["ParliamentScraping:RequestTimeoutSeconds"] ?? "30");
            _maxRetries = int.Parse(_configuration["ParliamentScraping:MaxRetries"] ?? "3");

            _httpClient.Timeout = TimeSpan.FromSeconds(_requestTimeout);
        }

        public override async Task<List<WatchlistEntry>> FetchWatchlistDataAsync()
        {
            try
            {
                _logger.LogInformation("Fetching Indian Parliament watchlist data via web scraping");
                
                var allEntries = new List<WatchlistEntry>();
                
                // Configure HttpClient for scraping
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                
                // Scrape different Parliament lists
                var lokSabhaMembers = await ScrapeParliamentListAsync(_lokSabhaMembers, "Lok Sabha Member");
                allEntries.AddRange(lokSabhaMembers);
                
                var rajyaSabhaMembers = await ScrapeParliamentListAsync(_rajyaSabhaMembers, "Rajya Sabha Member");
                allEntries.AddRange(rajyaSabhaMembers);
                
                var ministers = await ScrapeParliamentListAsync(_parliamentMinisters, "Minister");
                allEntries.AddRange(ministers);
                
                var committeeMembers = await ScrapeParliamentListAsync(_parliamentCommittees, "Committee Member");
                allEntries.AddRange(committeeMembers);
                
                _logger.LogInformation("Successfully scraped {Count} Indian Parliament entries", allEntries.Count);
                
                return allEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Indian Parliament watchlist data");
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
                Description = "Indian Parliament members including Lok Sabha, Rajya Sabha, Ministers, and Committee members",
                IsActive = true,
                UpdateFrequency = "Weekly",
                DataFormat = "Web Scraping",
                WebScrapingUrl = _parliamentBaseUrl
            };
        }

        /// <summary>
        /// Scrapes Parliament list data from the specified URL
        /// </summary>
        private async Task<List<WatchlistEntry>> ScrapeParliamentListAsync(string url, string category)
        {
            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Scraping Parliament list: {Category} from {Url} (attempt {Attempt}/{MaxRetries})", 
                        category, url, attempt, _maxRetries);
                    
                    var response = await _httpClient.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(response);
                    
                    var entries = new List<WatchlistEntry>();
                    
                    // Look for table elements that contain the member data
                    var tables = doc.DocumentNode.SelectNodes("//table[@class='table'] | //table[contains(@class, 'table')] | //table[@id='members-table'] | //div[contains(@class, 'member-card')]");
                    
                    if (tables == null || !tables.Any())
                    {
                        _logger.LogWarning("No tables or member cards found for category: {Category}", category);
                        return entries;
                    }
                    
                    foreach (var table in tables)
                    {
                        // Check if it's a table or member card div
                        if (table.Name == "table")
                        {
                            var rows = table.SelectNodes(".//tr");
                            if (rows == null) continue;
                            
                            // Skip header row
                            for (int i = 1; i < rows.Count; i++)
                            {
                                var row = rows[i];
                                var cells = row.SelectNodes(".//td");
                                
                                if (cells == null || cells.Count < 2) continue;
                                
                                var entry = ParseParliamentTableRow(cells, category);
                                if (entry != null && !string.IsNullOrEmpty(entry.PrimaryName))
                                {
                                    entries.Add(entry);
                                }
                            }
                        }
                        else if (table.Name == "div" && table.GetAttributeValue("class", "").Contains("member-card"))
                        {
                            // Parse member card format
                            var entry = ParseParliamentMemberCard(table, category);
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
                    _logger.LogError(ex, "Error scraping Parliament list for category: {Category} (attempt {Attempt}/{MaxRetries})", 
                        category, attempt, _maxRetries);
                    
                    if (attempt == _maxRetries)
                    {
                        _logger.LogError("Failed to scrape Parliament list for category: {Category} after {MaxRetries} attempts", 
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
        /// Parses Parliament table row data into a WatchlistEntry
        /// </summary>
        private WatchlistEntry? ParseParliamentTableRow(HtmlNodeCollection cells, string category)
        {
            try
            {
                if (cells.Count < 2) return null;

                var entry = new WatchlistEntry
                {
                    Source = SourceName,
                    ListType = "PEP",
                    RiskCategory = category,
                    RiskLevel = GetRiskLevelForCategory(category),
                    EntityType = "Individual",
                    Country = "India",
                    PepCategory = "Domestic",
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
                        // Try to parse as constituency/state, if not, use as party
                        if (secondCell.Contains("Constituency") || secondCell.Contains("State"))
                        {
                            entry.Address = secondCell;
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
                        if (thirdCell.Contains("Party"))
                        {
                            entry.PositionOrRole = thirdCell;
                        }
                        else
                        {
                            entry.PepDescription = thirdCell;
                        }
                    }
                }

                if (cells.Count >= 4)
                {
                    entry.PepDescription = cells[3].InnerText.Trim();
                }

                // Generate external ID if not available
                if (string.IsNullOrEmpty(entry.ExternalId))
                {
                    entry.ExternalId = $"PARL_{category.Replace(" ", "_")}_{Guid.NewGuid().ToString("N")[..8]}";
                }

                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Parliament table row");
                return null;
            }
        }

        /// <summary>
        /// Parses Parliament member card data into a WatchlistEntry
        /// </summary>
        private WatchlistEntry? ParseParliamentMemberCard(HtmlNode memberCard, string category)
        {
            try
            {
                var entry = new WatchlistEntry
                {
                    Source = SourceName,
                    ListType = "PEP",
                    RiskCategory = category,
                    RiskLevel = GetRiskLevelForCategory(category),
                    EntityType = "Individual",
                    Country = "India",
                    PepCategory = "Domestic",
                    DateAddedUtc = DateTime.UtcNow
                };

                // Extract name from member card
                var nameNode = memberCard.SelectSingleNode(".//h3 | .//h4 | .//div[contains(@class, 'name')] | .//span[contains(@class, 'name')]");
                if (nameNode != null)
                {
                    entry.PrimaryName = nameNode.InnerText.Trim();
                }

                // Extract constituency/state
                var constituencyNode = memberCard.SelectSingleNode(".//div[contains(@class, 'constituency')] | .//span[contains(@class, 'constituency')] | .//div[contains(text(), 'Constituency')]");
                if (constituencyNode != null)
                {
                    entry.Address = constituencyNode.InnerText.Trim();
                }

                // Extract party
                var partyNode = memberCard.SelectSingleNode(".//div[contains(@class, 'party')] | .//span[contains(@class, 'party')] | .//div[contains(text(), 'Party')]");
                if (partyNode != null)
                {
                    entry.PositionOrRole = partyNode.InnerText.Trim();
                }

                // Extract additional details
                var detailsNode = memberCard.SelectSingleNode(".//div[contains(@class, 'details')] | .//div[contains(@class, 'info')]");
                if (detailsNode != null)
                {
                    entry.PepDescription = detailsNode.InnerText.Trim();
                }

                // Generate external ID if not available
                if (string.IsNullOrEmpty(entry.ExternalId))
                {
                    entry.ExternalId = $"PARL_{category.Replace(" ", "_")}_{Guid.NewGuid().ToString("N")[..8]}";
                }

                return !string.IsNullOrEmpty(entry.PrimaryName) ? entry : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Parliament member card");
                return null;
            }
        }

        /// <summary>
        /// Determines risk level based on Parliament category
        /// </summary>
        private string GetRiskLevelForCategory(string category)
        {
            return category.ToLowerInvariant() switch
            {
                "minister" => "High",
                "lok sabha member" => "Medium",
                "rajya sabha member" => "Medium",
                "committee member" => "Low",
                _ => "Medium"
            };
        }

        /// <summary>
        /// Override CSV parsing for Parliament-specific format
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
                        ListType = "PEP",
                        PrimaryName = csv.GetField("Name") ?? csv.GetField("MemberName") ?? csv.GetField("MPName") ?? "",
                        PositionOrRole = csv.GetField("Party") ?? csv.GetField("PoliticalParty") ?? "",
                        Address = csv.GetField("Constituency") ?? csv.GetField("State") ?? "",
                        Country = "India",
                        EntityType = "Individual",
                        RiskCategory = csv.GetField("Category") ?? csv.GetField("Type") ?? "Parliament Member",
                        RiskLevel = GetRiskLevelForCategory(csv.GetField("Category") ?? ""),
                        PepCategory = "Domestic",
                        PepDescription = csv.GetField("Description") ?? csv.GetField("Details") ?? "",
                        ExternalId = csv.GetField("MP_ID") ?? csv.GetField("ID") ?? "",
                        DateAddedUtc = DateTime.UtcNow
                    };

                    // Parse dates if available
                    if (DateTime.TryParse(csv.GetField("StartDate"), out var startDate))
                        entry.PepStartDate = startDate;

                    if (DateTime.TryParse(csv.GetField("EndDate"), out var endDate))
                        entry.PepEndDate = endDate;

                    if (!string.IsNullOrEmpty(entry.PrimaryName))
                    {
                        entries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Parliament CSV entries");
            }

            return entries;
        }

        /// <summary>
        /// Override Excel parsing for Parliament-specific format
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
                    _logger.LogWarning("No worksheet found in Parliament Excel file");
                    return entries;
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2) return entries; // Need at least header + 1 data row

                // Find column indices
                var nameCol = FindColumnIndex(worksheet, "Name", "MemberName", "MPName");
                var partyCol = FindColumnIndex(worksheet, "Party", "PoliticalParty");
                var constituencyCol = FindColumnIndex(worksheet, "Constituency", "State", "District");
                var categoryCol = FindColumnIndex(worksheet, "Category", "Type", "House");
                var descriptionCol = FindColumnIndex(worksheet, "Description", "Details", "Remarks");
                var startDateCol = FindColumnIndex(worksheet, "StartDate", "FromDate", "ElectedDate");
                var endDateCol = FindColumnIndex(worksheet, "EndDate", "ToDate", "TermEndDate");
                var idCol = FindColumnIndex(worksheet, "MP_ID", "ID", "Reference");

                for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip header)
                {
                    var name = worksheet.Cells[row, nameCol].Value?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(name)) continue;

                    var entry = new WatchlistEntry
                    {
                        Source = SourceName,
                        ListType = "PEP",
                        PrimaryName = name,
                        PositionOrRole = worksheet.Cells[row, partyCol].Value?.ToString()?.Trim() ?? "",
                        Address = worksheet.Cells[row, constituencyCol].Value?.ToString()?.Trim() ?? "",
                        Country = "India",
                        EntityType = "Individual",
                        RiskCategory = worksheet.Cells[row, categoryCol].Value?.ToString()?.Trim() ?? "Parliament Member",
                        RiskLevel = GetRiskLevelForCategory(worksheet.Cells[row, categoryCol].Value?.ToString() ?? ""),
                        PepCategory = "Domestic",
                        PepDescription = worksheet.Cells[row, descriptionCol].Value?.ToString()?.Trim() ?? "",
                        ExternalId = worksheet.Cells[row, idCol].Value?.ToString()?.Trim() ?? "",
                        DateAddedUtc = DateTime.UtcNow
                    };

                    // Parse dates
                    if (DateTime.TryParse(worksheet.Cells[row, startDateCol].Value?.ToString(), out var startDate))
                        entry.PepStartDate = startDate;

                    if (DateTime.TryParse(worksheet.Cells[row, endDateCol].Value?.ToString(), out var endDate))
                        entry.PepEndDate = endDate;

                    entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Parliament Excel entries");
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
        /// Advanced Parliament scraping to find additional data sources
        /// </summary>
        public async Task<WatchlistUpdateResult> ScrapeParliamentAdvancedAsync()
        {
            var result = new WatchlistUpdateResult
            {
                Source = SourceName,
                ProcessingDate = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting advanced Parliament scraping");

                // Scrape main Parliament page to find additional links
                var mainPageUrl = _parliamentBaseUrl;
                var response = await _httpClient.GetStringAsync(mainPageUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // Look for links that might contain member data
                var links = doc.DocumentNode.SelectNodes("//a[contains(@href, 'member') or contains(@href, 'mp') or contains(@href, 'minister') or contains(@href, 'committee')]");
                
                if (links != null)
                {
                    foreach (var link in links.Take(10)) // Limit to first 10 links
                    {
                        var href = link.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            _logger.LogInformation("Found potential Parliament data link: {Link}", href);
                        }
                    }
                }

                result.Success = true;
                result.TotalRecords = 0; // This is just for discovery
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced Parliament scraping");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Search Parliament data by category
        /// </summary>
        public async Task<List<WatchlistEntry>> SearchParliamentByCategoryAsync(string category)
        {
            try
            {
                var url = category.ToLowerInvariant() switch
                {
                    "lok sabha" or "lok sabha member" => _lokSabhaMembers,
                    "rajya sabha" or "rajya sabha member" => _rajyaSabhaMembers,
                    "minister" or "ministers" => _parliamentMinisters,
                    "committee" or "committee member" => _parliamentCommittees,
                    _ => _lokSabhaMembers // Default
                };

                return await ScrapeParliamentListAsync(url, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Parliament data by category: {Category}", category);
                throw;
            }
        }
    }
}
