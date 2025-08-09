using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using CsvHelper;
using System.Globalization;
using System.Text.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace PEPScanner.Application.Services
{
    public interface IRbiWatchlistService : IBaseWatchlistService
    {
        Task<List<RbiEntry>> FetchRbiWatchlistAsync();
        Task<WatchlistUpdateResult> UpdateWatchlistFromRbiAsync();
        Task<WatchlistUpdateResult> ProcessRbiFileAsync(IFormFile file);
        Task<WatchlistUpdateResult> ScrapeRbiAdvancedAsync();
        Task<List<RbiEntry>> SearchRbiByCategoryAsync(string category);
    }

    public class RbiWatchlistService : BaseWatchlistService, IRbiWatchlistService
    {
        // RBI Watchlist endpoints and scraping configuration
        private readonly string _rbiBaseUrl;
        private readonly string _rbiWilfulDefaulters;
        private readonly string _rbiFraudMaster;
        private readonly string _rbiCautionList;
        private readonly string _rbiDefaultersList;
        private readonly string _rbiAlertList;
        private readonly string _userAgent;
        private readonly int _requestTimeout;
        private readonly int _maxRetries;

        // IBaseWatchlistService properties
        public override string SourceName => "RBI";
        public override string DisplayName => "Reserve Bank of India";
        public override string WatchlistType => "Local";
        public override string Country => "India";

        public RbiWatchlistService(
            PepScannerDbContext context,
            ILogger<RbiWatchlistService> logger,
            IConfiguration configuration,
            HttpClient httpClient) : base(context, logger, configuration, httpClient)
        {
            
            // Load configuration from appsettings
            _rbiBaseUrl = _configuration["RbiScraping:BaseUrl"] ?? "https://www.rbi.org.in";
            _rbiWilfulDefaulters = _configuration["RbiScraping:WilfulDefaultersUrl"] ?? "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2009";
            _rbiFraudMaster = _configuration["RbiScraping:FraudMasterUrl"] ?? "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2010";
            _rbiCautionList = _configuration["RbiScraping:CautionListUrl"] ?? "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2011";
            _rbiDefaultersList = _configuration["RbiScraping:DefaultersListUrl"] ?? "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2008";
            _rbiAlertList = _configuration["RbiScraping:AlertListUrl"] ?? "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2012";
            _userAgent = _configuration["RbiScraping:UserAgent"] ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
            _requestTimeout = int.TryParse(_configuration["RbiScraping:RequestTimeoutSeconds"], out var timeout) ? timeout : 30;
            _maxRetries = int.TryParse(_configuration["RbiScraping:MaxRetries"], out var retries) ? retries : 3;
            
            // Configure HttpClient timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(_requestTimeout);
        }

        public override WatchlistSource GetSourceConfiguration()
        {
            return new WatchlistSource
            {
                Name = SourceName,
                DisplayName = DisplayName,
                Type = WatchlistType,
                Country = Country,
                Description = "RBI regulatory lists including wilful defaulters, fraud master, caution list, defaulters list, and alert list",
                IsActive = true,
                UpdateFrequency = "Daily",
                DataFormat = "Web Scraping",
                WebScrapingUrl = _rbiBaseUrl
            };
        }

        public override async Task<List<WatchlistEntry>> FetchWatchlistDataAsync()
        {
            var rbiEntries = await FetchRbiWatchlistAsync();
            return rbiEntries.Select(CreateWatchlistEntryFromRbi).ToList();
        }

        public async Task<List<RbiEntry>> FetchRbiWatchlistAsync()
        {
            try
            {
                _logger.LogInformation("Fetching RBI watchlist data via web scraping");
                
                var allEntries = new List<RbiEntry>();
                
                // Configure HttpClient for scraping
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                
                // Scrape different RBI lists
                var wilfulDefaulters = await ScrapeRbiListAsync(_rbiWilfulDefaulters, "Wilful Defaulter");
                allEntries.AddRange(wilfulDefaulters);
                
                var fraudMaster = await ScrapeRbiListAsync(_rbiFraudMaster, "Fraud Master");
                allEntries.AddRange(fraudMaster);
                
                var cautionList = await ScrapeRbiListAsync(_rbiCautionList, "Caution List");
                allEntries.AddRange(cautionList);
                
                var defaultersList = await ScrapeRbiListAsync(_rbiDefaultersList, "Defaulters List");
                allEntries.AddRange(defaultersList);
                
                var alertList = await ScrapeRbiListAsync(_rbiAlertList, "Alert List");
                allEntries.AddRange(alertList);
                
                _logger.LogInformation("Successfully scraped {Count} RBI entries", allEntries.Count);
                
                return allEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching RBI watchlist data");
                throw;
            }
        }

        public async Task<WatchlistUpdateResult> UpdateWatchlistFromRbiAsync()
        {
            var result = new WatchlistUpdateResult
            {
                Source = "RBI",
                ProcessingDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Starting RBI watchlist update");
                
                var rbiEntries = await FetchRbiWatchlistAsync();
                result.TotalRecords = rbiEntries.Count;

                foreach (var rbiEntry in rbiEntries)
                {
                    if (string.IsNullOrEmpty(rbiEntry.Name))
                        continue;

                    var existingEntry = await _context.WatchlistEntries
                        .FirstOrDefaultAsync(w => w.ExternalId == rbiEntry.Id && w.Source == "RBI");

                    if (existingEntry == null)
                    {
                        // Create new watchlist entry
                        var newEntry = new WatchlistEntry
                        {
                            Id = Guid.NewGuid(),
                            Source = "RBI",
                            ListType = GetListType(rbiEntry.Category),
                            PrimaryName = rbiEntry.Name,
                            EntityType = "Individual",
                            RiskLevel = GetRiskLevel(rbiEntry.Category),
                            RiskCategory = rbiEntry.Category,
                            Country = "India",
                            Nationality = "Indian",
                            Citizenship = "Indian",
                            DateOfBirth = ParseDateOfBirth(rbiEntry.DateOfBirth),
                            Address = rbiEntry.Address,
                            City = rbiEntry.City,
                            State = rbiEntry.State,
                            PostalCode = rbiEntry.PostalCode,
                            PositionOrRole = rbiEntry.Designation,
                            PepPosition = rbiEntry.Designation,
                            PepCountry = "India",
                            SanctionType = GetSanctionType(rbiEntry.Category),
                            SanctionAuthority = "Reserve Bank of India",
                            SanctionReference = rbiEntry.Id,
                            SanctionReason = rbiEntry.Reason,
                            ExternalId = rbiEntry.Id,
                            ExternalReference = rbiEntry.Id,
                            Comments = rbiEntry.Remarks,
                            DateAddedUtc = DateTime.UtcNow,
                            IsActive = true,
                            AddedBy = "System"
                        };

                        // Add alternate names
                        if (!string.IsNullOrEmpty(rbiEntry.Aliases))
                        {
                            newEntry.AlternateNames = rbiEntry.Aliases;
                        }

                        _context.WatchlistEntries.Add(newEntry);
                        result.NewRecords++;
                    }
                    else
                    {
                        // Update existing entry
                        existingEntry.PrimaryName = rbiEntry.Name;
                        existingEntry.DateOfBirth = ParseDateOfBirth(rbiEntry.DateOfBirth);
                        existingEntry.Address = rbiEntry.Address;
                        existingEntry.City = rbiEntry.City;
                        existingEntry.State = rbiEntry.State;
                        existingEntry.PostalCode = rbiEntry.PostalCode;
                        existingEntry.PositionOrRole = rbiEntry.Designation;
                        existingEntry.SanctionReason = rbiEntry.Reason;
                        existingEntry.Comments = rbiEntry.Remarks;
                        existingEntry.DateLastUpdatedUtc = DateTime.UtcNow;
                        existingEntry.UpdatedBy = "System";

                        if (!string.IsNullOrEmpty(rbiEntry.Aliases))
                        {
                            existingEntry.AlternateNames = rbiEntry.Aliases;
                        }

                        _context.WatchlistEntries.Update(existingEntry);
                        result.UpdatedRecords++;
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                
                _logger.LogInformation("RBI watchlist update completed. {Result}", result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating watchlist from RBI");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        public override async Task<List<WatchlistEntry>> SearchByNameAsync(string name)
        {
            try
            {
                _logger.LogInformation("Searching RBI data for name: {Name}", name);
                
                var watchlistEntries = await _context.WatchlistEntries
                    .Where(w => w.Source == "RBI" && 
                               (w.PrimaryName.Contains(name) || 
                                w.AlternateNames.Contains(name)))
                    .ToListAsync();
                
                return watchlistEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching RBI data for name: {Name}", name);
                throw;
            }
        }

        public async Task<WatchlistUpdateResult> ProcessRbiFileAsync(IFormFile file)
        {
            var result = new WatchlistUpdateResult
            {
                Source = "RBI",
                ProcessingDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Processing RBI file: {FileName}", file.FileName);
                
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                
                csv.Context.RegisterClassMap<RbiEntryMap>();
                var rbiEntries = csv.GetRecords<RbiEntry>().ToList();
                
                result.TotalRecords = rbiEntries.Count;

                foreach (var rbiEntry in rbiEntries)
                {
                    if (string.IsNullOrEmpty(rbiEntry.Name))
                        continue;

                    var existingEntry = await _context.WatchlistEntries
                        .FirstOrDefaultAsync(w => w.ExternalId == rbiEntry.Id && w.Source == "RBI");

                    if (existingEntry == null)
                    {
                        var newEntry = CreateWatchlistEntryFromRbi(rbiEntry);
                        _context.WatchlistEntries.Add(newEntry);
                        result.NewRecords++;
                    }
                    else
                    {
                        UpdateWatchlistEntryFromRbi(existingEntry, rbiEntry);
                        _context.WatchlistEntries.Update(existingEntry);
                        result.UpdatedRecords++;
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                
                _logger.LogInformation("RBI file processing completed. {Result}", result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RBI file");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        public async Task<WatchlistUpdateResult> ScrapeRbiAdvancedAsync()
        {
            var result = new WatchlistUpdateResult
            {
                Source = "RBI Advanced Scraping",
                ProcessingDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Starting advanced RBI scraping");
                
                var advancedEntries = await ScrapeRbiAdvancedDataAsync();
                result.TotalRecords = advancedEntries.Count;

                foreach (var rbiEntry in advancedEntries)
                {
                    if (string.IsNullOrEmpty(rbiEntry.Name))
                        continue;

                    var existingEntry = await _context.WatchlistEntries
                        .FirstOrDefaultAsync(w => w.ExternalId == rbiEntry.Id && w.Source == "RBI");

                    if (existingEntry == null)
                    {
                        var newEntry = CreateWatchlistEntryFromRbi(rbiEntry);
                        _context.WatchlistEntries.Add(newEntry);
                        result.NewRecords++;
                    }
                    else
                    {
                        UpdateWatchlistEntryFromRbi(existingEntry, rbiEntry);
                        _context.WatchlistEntries.Update(existingEntry);
                        result.UpdatedRecords++;
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                
                _logger.LogInformation("Advanced RBI scraping completed. {Result}", result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced RBI scraping");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        public async Task<List<RbiEntry>> SearchRbiByCategoryAsync(string category)
        {
            try
            {
                _logger.LogInformation("Searching RBI data by category: {Category}", category);
                
                var url = category.ToLower() switch
                {
                    "wilful defaulter" => _rbiWilfulDefaulters,
                    "fraud master" => _rbiFraudMaster,
                    "caution list" => _rbiCautionList,
                    "defaulters list" => _rbiDefaultersList,
                    "alert list" => _rbiAlertList,
                    _ => _rbiWilfulDefaulters // Default
                };
                
                var entries = await ScrapeRbiListAsync(url, category);
                
                _logger.LogInformation("Found {Count} entries for category: {Category}", entries.Count, category);
                
                return entries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching RBI data by category: {Category}", category);
                throw;
            }
        }

        private string GetListType(string? category)
        {
            return category?.ToLower() switch
            {
                "wilful defaulter" => "Wilful Defaulters",
                "fraud" => "Fraud Master",
                "caution" => "Caution List",
                _ => "Local Lists"
            };
        }

        private string GetRiskLevel(string? category)
        {
            return category?.ToLower() switch
            {
                "wilful defaulter" => "High",
                "fraud" => "Critical",
                "caution" => "Medium",
                _ => "Medium"
            };
        }

        private string GetSanctionType(string? category)
        {
            return category?.ToLower() switch
            {
                "wilful defaulter" => "Credit Restriction",
                "fraud" => "Banking Ban",
                "caution" => "Enhanced Monitoring",
                _ => "Enhanced Monitoring"
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

        private WatchlistEntry CreateWatchlistEntryFromRbi(RbiEntry rbiEntry)
        {
            return new WatchlistEntry
            {
                Id = Guid.NewGuid(),
                Source = "RBI",
                ListType = GetListType(rbiEntry.Category),
                PrimaryName = rbiEntry.Name ?? "",
                EntityType = "Individual",
                RiskLevel = GetRiskLevel(rbiEntry.Category),
                RiskCategory = rbiEntry.Category ?? "",
                Country = "India",
                Nationality = "Indian",
                Citizenship = "Indian",
                DateOfBirth = ParseDateOfBirth(rbiEntry.DateOfBirth),
                Address = rbiEntry.Address,
                City = rbiEntry.City,
                State = rbiEntry.State,
                PostalCode = rbiEntry.PostalCode,
                PositionOrRole = rbiEntry.Designation,
                PepPosition = rbiEntry.Designation,
                PepCountry = "India",
                SanctionType = GetSanctionType(rbiEntry.Category),
                SanctionAuthority = "Reserve Bank of India",
                SanctionReference = rbiEntry.Id,
                SanctionReason = rbiEntry.Reason,
                ExternalId = rbiEntry.Id,
                ExternalReference = rbiEntry.Id,
                Comments = rbiEntry.Remarks,
                DateAddedUtc = DateTime.UtcNow,
                IsActive = true,
                AddedBy = "System",
                AlternateNames = rbiEntry.Aliases
            };
        }

        private void UpdateWatchlistEntryFromRbi(WatchlistEntry existingEntry, RbiEntry rbiEntry)
        {
            existingEntry.PrimaryName = rbiEntry.Name ?? existingEntry.PrimaryName;
            existingEntry.DateOfBirth = ParseDateOfBirth(rbiEntry.DateOfBirth) ?? existingEntry.DateOfBirth;
            existingEntry.Address = rbiEntry.Address ?? existingEntry.Address;
            existingEntry.City = rbiEntry.City ?? existingEntry.City;
            existingEntry.State = rbiEntry.State ?? existingEntry.State;
            existingEntry.PostalCode = rbiEntry.PostalCode ?? existingEntry.PostalCode;
            existingEntry.PositionOrRole = rbiEntry.Designation ?? existingEntry.PositionOrRole;
            existingEntry.SanctionReason = rbiEntry.Reason ?? existingEntry.SanctionReason;
            existingEntry.Comments = rbiEntry.Remarks ?? existingEntry.Comments;
            existingEntry.AlternateNames = rbiEntry.Aliases ?? existingEntry.AlternateNames;
            existingEntry.DateLastUpdatedUtc = DateTime.UtcNow;
            existingEntry.UpdatedBy = "System";
        }

        private async Task<List<RbiEntry>> ScrapeRbiListAsync(string url, string category)
        {
            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Scraping RBI list: {Category} from {Url} (attempt {Attempt}/{MaxRetries})", 
                        category, url, attempt, _maxRetries);
                    
                    var response = await _httpClient.GetStringAsync(url);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(response);
                
                var entries = new List<RbiEntry>();
                
                // Look for table elements that contain the watchlist data
                var tables = doc.DocumentNode.SelectNodes("//table[@class='tablebg'] | //table[@class='table'] | //table[contains(@class, 'table')]");
                
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
                        
                        var entry = ParseRbiTableRow(cells, category);
                        if (entry != null && !string.IsNullOrEmpty(entry.Name))
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
                    _logger.LogError(ex, "Error scraping RBI list for category: {Category} (attempt {Attempt}/{MaxRetries})", 
                        category, attempt, _maxRetries);
                    
                    if (attempt == _maxRetries)
                    {
                        _logger.LogError("Failed to scrape RBI list for category: {Category} after {MaxRetries} attempts", 
                            category, _maxRetries);
                        return new List<RbiEntry>();
                    }
                    
                    // Wait before retrying
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
                }
            }
            
            return new List<RbiEntry>();
        }

        private RbiEntry? ParseRbiTableRow(HtmlNodeCollection cells, string category)
        {
            try
            {
                var entry = new RbiEntry
                {
                    Category = category,
                    ListedDate = DateTime.UtcNow
                };
                
                // Extract data from cells based on typical RBI table structure
                for (int i = 0; i < cells.Count; i++)
                {
                    var cellText = cells[i].InnerText?.Trim();
                    if (string.IsNullOrEmpty(cellText)) continue;
                    
                    // Remove extra whitespace and newlines
                    cellText = Regex.Replace(cellText, @"\s+", " ");
                    
                    switch (i)
                    {
                        case 0: // Usually ID or Serial Number
                            if (int.TryParse(cellText, out var id))
                                entry.Id = id.ToString();
                            break;
                        case 1: // Usually Name
                            entry.Name = cellText;
                            break;
                        case 2: // Usually Address or Designation
                            if (cellText.Contains(",") || cellText.Contains("Street") || cellText.Contains("Road"))
                                entry.Address = cellText;
                            else
                                entry.Designation = cellText;
                            break;
                        case 3: // Usually City/State or Additional Info
                            if (cellText.Contains(","))
                            {
                                var parts = cellText.Split(',');
                                if (parts.Length >= 2)
                                {
                                    entry.City = parts[0].Trim();
                                    entry.State = parts[1].Trim();
                                }
                            }
                            else
                            {
                                entry.City = cellText;
                            }
                            break;
                        case 4: // Usually Amount or Additional Details
                            if (decimal.TryParse(Regex.Replace(cellText, @"[^\d.]", ""), out var amount))
                                entry.Amount = amount;
                            else
                                entry.Remarks = cellText;
                            break;
                        case 5: // Usually Bank Name or Additional Info
                            if (cellText.Contains("Bank") || cellText.Contains("Ltd") || cellText.Contains("Corp"))
                                entry.BankName = cellText;
                            else
                                entry.Reason = cellText;
                            break;
                    }
                }
                
                // Generate a unique ID if not present
                if (string.IsNullOrEmpty(entry.Id))
                {
                    entry.Id = $"RBI_{category.Replace(" ", "_")}_{Guid.NewGuid().ToString("N")[..8]}";
                }
                
                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing RBI table row");
                return null;
            }
        }

        private async Task<List<RbiEntry>> ScrapeRbiAdvancedDataAsync()
        {
            try
            {
                _logger.LogInformation("Performing advanced RBI scraping");
                
                var entries = new List<RbiEntry>();
                
                // Try to find additional RBI pages with watchlist data
                var searchUrls = new[]
                {
                    $"{_rbiBaseUrl}/Scripts/bs_viewcontent.aspx?Id=2007", // Additional defaulters
                    $"{_rbiBaseUrl}/Scripts/bs_viewcontent.aspx?Id=2013", // Additional alerts
                    $"{_rbiBaseUrl}/Scripts/bs_viewcontent.aspx?Id=2014", // Additional fraud cases
                };
                
                foreach (var url in searchUrls)
                {
                    try
                    {
                        var response = await _httpClient.GetStringAsync(url);
                        var doc = new HtmlDocument();
                        doc.LoadHtml(response);
                        
                        // Look for links to PDF files or additional data
                        var links = doc.DocumentNode.SelectNodes("//a[contains(@href, '.pdf') or contains(@href, '.xls') or contains(@href, '.csv')]");
                        
                        if (links != null)
                        {
                            foreach (var link in links)
                            {
                                var href = link.GetAttributeValue("href", "");
                                if (!string.IsNullOrEmpty(href))
                                {
                                    var fullUrl = href.StartsWith("http") ? href : $"{_rbiBaseUrl}{href}";
                                    _logger.LogInformation("Found RBI data file: {Url}", fullUrl);
                                    
                                    // Note: In a real implementation, you might want to download and parse these files
                                    // For now, we'll just log them
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error accessing RBI URL: {Url}", url);
                    }
                }
                
                return entries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced RBI scraping");
                return new List<RbiEntry>();
            }
        }
    }

    public class RbiEntry
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Designation { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Aliases { get; set; }
        public string? Remarks { get; set; }
        public string? Reason { get; set; }
        public DateTime? ListedDate { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public decimal? Amount { get; set; }
    }

    public sealed class RbiEntryMap : CsvHelper.Configuration.ClassMap<RbiEntry>
    {
        public RbiEntryMap()
        {
            Map(m => m.Id).Name("ID", "Id", "id");
            Map(m => m.Name).Name("Name", "NAME", "name");
            Map(m => m.Category).Name("Category", "CATEGORY", "category");
            Map(m => m.Designation).Name("Designation", "DESIGNATION", "designation");
            Map(m => m.DateOfBirth).Name("DateOfBirth", "DOB", "dob", "Date of Birth");
            Map(m => m.Address).Name("Address", "ADDRESS", "address");
            Map(m => m.City).Name("City", "CITY", "city");
            Map(m => m.State).Name("State", "STATE", "state");
            Map(m => m.PostalCode).Name("PostalCode", "PIN", "pin", "Postal Code");
            Map(m => m.Aliases).Name("Aliases", "ALIASES", "aliases");
            Map(m => m.Remarks).Name("Remarks", "REMARKS", "remarks");
            Map(m => m.Reason).Name("Reason", "REASON", "reason");
            Map(m => m.ListedDate).Name("ListedDate", "LISTED_DATE", "listed_date");
            Map(m => m.BankName).Name("BankName", "BANK_NAME", "bank_name");
            Map(m => m.AccountNumber).Name("AccountNumber", "ACCOUNT_NUMBER", "account_number");
            Map(m => m.Amount).Name("Amount", "AMOUNT", "amount");
        }
    }
}
