using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.API.Models;
using CsvHelper;
using System.Globalization;
using System.Text.Json;

namespace PEPScanner.API.Services
{
    /// <summary>
    /// Abstract base class for watchlist services providing common functionality
    /// </summary>
    public abstract class BaseWatchlistService : IBaseWatchlistService
    {
        protected readonly PepScannerDbContext _context;
        protected readonly ILogger _logger;
        protected readonly IConfiguration _configuration;
        protected readonly HttpClient _httpClient;

        protected BaseWatchlistService(
            PepScannerDbContext context,
            ILogger logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // Abstract properties that must be implemented by derived classes
        public abstract string SourceName { get; }
        public abstract string DisplayName { get; }
        public abstract string WatchlistType { get; }
        public abstract string Country { get; }

        // Abstract methods that must be implemented by derived classes
        public abstract Task<List<WatchlistEntry>> FetchWatchlistDataAsync();
        public abstract WatchlistSource GetSourceConfiguration();

        /// <summary>
        /// Default implementation for updating watchlist from fetched data
        /// </summary>
        public virtual async Task<WatchlistUpdateResult> UpdateWatchlistAsync()
        {
            var result = new WatchlistUpdateResult
            {
                Source = SourceName,
                ProcessingDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Starting {Source} watchlist update", SourceName);
                
                var watchlistEntries = await FetchWatchlistDataAsync();
                result.TotalRecords = watchlistEntries.Count;

                foreach (var entry in watchlistEntries)
                {
                    if (string.IsNullOrEmpty(entry.PrimaryName))
                        continue;

                    var existingEntry = await _context.WatchlistEntries
                        .FirstOrDefaultAsync(w => w.ExternalId == entry.ExternalId && w.Source == SourceName);

                    if (existingEntry == null)
                    {
                        // Add new entry
                        entry.Id = Guid.NewGuid();
                        entry.DateAddedUtc = DateTime.UtcNow;
                        entry.AddedBy = "System";
                        _context.WatchlistEntries.Add(entry);
                        result.NewRecords++;
                    }
                    else
                    {
                        // Update existing entry
                        UpdateWatchlistEntry(existingEntry, entry);
                        existingEntry.DateLastUpdatedUtc = DateTime.UtcNow;
                        existingEntry.UpdatedBy = "System";
                        result.UpdatedRecords++;
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;

                _logger.LogInformation("Successfully updated {Source} watchlist: {NewRecords} new, {UpdatedRecords} updated", 
                    SourceName, result.NewRecords, result.UpdatedRecords);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {Source} watchlist", SourceName);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        /// <summary>
        /// Default implementation for searching by name
        /// </summary>
        public virtual async Task<List<WatchlistEntry>> SearchByNameAsync(string name)
        {
            try
            {
                return await _context.WatchlistEntries
                    .Where(w => w.Source == SourceName && w.IsActive && !w.IsWhitelisted)
                    .Where(w => w.PrimaryName.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                               (w.AlternateNames != null && w.AlternateNames.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching {Source} data for name: {Name}", SourceName, name);
                throw;
            }
        }

        /// <summary>
        /// Default implementation for processing file uploads
        /// </summary>
        public virtual async Task<WatchlistUpdateResult> ProcessFileAsync(IFormFile file)
        {
            var result = new WatchlistUpdateResult
            {
                Source = SourceName,
                ProcessingDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Processing {Source} file: {FileName}", SourceName, file.FileName);
                
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                switch (fileExtension)
                {
                    case ".csv":
                        return await ProcessCsvFileAsync(file);
                    case ".json":
                        return await ProcessJsonFileAsync(file);
                    case ".xlsx":
                    case ".xls":
                        return await ProcessExcelFileAsync(file);
                    default:
                        throw new NotSupportedException($"File format {fileExtension} is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {Source} file: {FileName}", SourceName, file.FileName);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        /// <summary>
        /// Default implementation for getting last update timestamp
        /// </summary>
        public virtual async Task<DateTime?> GetLastUpdateTimestampAsync()
        {
            try
            {
                var lastEntry = await _context.WatchlistEntries
                    .Where(w => w.Source == SourceName)
                    .OrderByDescending(w => w.DateLastUpdatedUtc ?? w.DateAddedUtc)
                    .FirstOrDefaultAsync();

                return lastEntry?.DateLastUpdatedUtc ?? lastEntry?.DateAddedUtc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last update timestamp for {Source}", SourceName);
                return null;
            }
        }

        /// <summary>
        /// Updates an existing watchlist entry with new data
        /// </summary>
        protected virtual void UpdateWatchlistEntry(WatchlistEntry existing, WatchlistEntry updated)
        {
            existing.PrimaryName = updated.PrimaryName;
            existing.AlternateNames = updated.AlternateNames;
            existing.Country = updated.Country;
            existing.Gender = updated.Gender;
            existing.DateOfBirth = updated.DateOfBirth;
            existing.DateOfBirthFrom = updated.DateOfBirthFrom;
            existing.DateOfBirthTo = updated.DateOfBirthTo;
            existing.PositionOrRole = updated.PositionOrRole;
            existing.RiskCategory = updated.RiskCategory;
            existing.RiskLevel = updated.RiskLevel;
            existing.EntityType = updated.EntityType;
            existing.Nationality = updated.Nationality;
            existing.Citizenship = updated.Citizenship;
            existing.Address = updated.Address;
            existing.City = updated.City;
            existing.State = updated.State;
            existing.PostalCode = updated.PostalCode;
            existing.CountryOfResidence = updated.CountryOfResidence;
            existing.PassportNumber = updated.PassportNumber;
            existing.NationalIdNumber = updated.NationalIdNumber;
            existing.TaxIdNumber = updated.TaxIdNumber;
            existing.RegistrationNumber = updated.RegistrationNumber;
            existing.LicenseNumber = updated.LicenseNumber;
            existing.PepPosition = updated.PepPosition;
            existing.PepCountry = updated.PepCountry;
            existing.PepStartDate = updated.PepStartDate;
            existing.PepEndDate = updated.PepEndDate;
            existing.PepCategory = updated.PepCategory;
            existing.PepDescription = updated.PepDescription;
            existing.SanctionType = updated.SanctionType;
            existing.SanctionAuthority = updated.SanctionAuthority;
            existing.SanctionReference = updated.SanctionReference;
            existing.SanctionStartDate = updated.SanctionStartDate;
            existing.SanctionEndDate = updated.SanctionEndDate;
            existing.SanctionReason = updated.SanctionReason;
            existing.MediaSource = updated.MediaSource;
            existing.MediaDate = updated.MediaDate;
            existing.MediaSummary = updated.MediaSummary;
            existing.MediaUrl = updated.MediaUrl;
            existing.MediaCategory = updated.MediaCategory;
            existing.ExternalReference = updated.ExternalReference;
            existing.Comments = updated.Comments;
            existing.DataQuality = updated.DataQuality;
            existing.IsActive = updated.IsActive;
        }

        /// <summary>
        /// Processes CSV file uploads
        /// </summary>
        protected virtual async Task<WatchlistUpdateResult> ProcessCsvFileAsync(IFormFile file)
        {
            var result = new WatchlistUpdateResult
            {
                Source = SourceName,
                ProcessingDate = DateTime.UtcNow
            };

            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                // This should be overridden by derived classes to provide specific CSV mapping
                var entries = await ParseCsvEntriesAsync(csv);
                
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count; // Simplified for now
                
                foreach (var entry in entries)
                {
                    entry.Id = Guid.NewGuid();
                    entry.Source = SourceName;
                    entry.DateAddedUtc = DateTime.UtcNow;
                    entry.AddedBy = "FileUpload";
                    _context.WatchlistEntries.Add(entry);
                }

                await _context.SaveChangesAsync();
                result.Success = true;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV file for {Source}", SourceName);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Processes JSON file uploads
        /// </summary>
        protected virtual async Task<WatchlistUpdateResult> ProcessJsonFileAsync(IFormFile file)
        {
            var result = new WatchlistUpdateResult
            {
                Source = SourceName,
                ProcessingDate = DateTime.UtcNow
            };

            try
            {
                using var stream = file.OpenReadStream();
                var jsonString = await new StreamReader(stream).ReadToEndAsync();
                var entries = JsonSerializer.Deserialize<List<WatchlistEntry>>(jsonString) ?? new List<WatchlistEntry>();
                
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;

                foreach (var entry in entries)
                {
                    entry.Id = Guid.NewGuid();
                    entry.Source = SourceName;
                    entry.DateAddedUtc = DateTime.UtcNow;
                    entry.AddedBy = "FileUpload";
                    _context.WatchlistEntries.Add(entry);
                }

                await _context.SaveChangesAsync();
                result.Success = true;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing JSON file for {Source}", SourceName);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Processes Excel file uploads
        /// </summary>
        protected virtual async Task<WatchlistUpdateResult> ProcessExcelFileAsync(IFormFile file)
        {
            var result = new WatchlistUpdateResult
            {
                Source = SourceName,
                ProcessingDate = DateTime.UtcNow
            };

            try
            {
                // This should be overridden by derived classes to provide specific Excel parsing
                var entries = await ParseExcelEntriesAsync(file);
                
                result.TotalRecords = entries.Count;
                result.NewRecords = entries.Count;

                foreach (var entry in entries)
                {
                    entry.Id = Guid.NewGuid();
                    entry.Source = SourceName;
                    entry.DateAddedUtc = DateTime.UtcNow;
                    entry.AddedBy = "FileUpload";
                    _context.WatchlistEntries.Add(entry);
                }

                await _context.SaveChangesAsync();
                result.Success = true;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Excel file for {Source}", SourceName);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Virtual method for parsing CSV entries - can be overridden by derived classes
        /// </summary>
        protected virtual async Task<List<WatchlistEntry>> ParseCsvEntriesAsync(CsvReader csv)
        {
            // Default implementation - should be overridden by derived classes
            return new List<WatchlistEntry>();
        }

        /// <summary>
        /// Virtual method for parsing Excel entries - can be overridden by derived classes
        /// </summary>
        protected virtual async Task<List<WatchlistEntry>> ParseExcelEntriesAsync(IFormFile file)
        {
            // Default implementation - should be overridden by derived classes
            return new List<WatchlistEntry>();
        }
    }
}
