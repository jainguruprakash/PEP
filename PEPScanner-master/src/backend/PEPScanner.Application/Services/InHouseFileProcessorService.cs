using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using CsvHelper;
using System.Globalization;
using System.Text.Json;
using OfficeOpenXml;
using System.Data;

namespace PEPScanner.Application.Services
{
    public interface IInHouseFileProcessorService
    {
        Task<WatchlistUpdateResult> ProcessFileAsync(IFormFile file, string sourceName, string? fileFormat = null);
        Task<List<string>> GetSupportedFormatsAsync();
        Task<FileValidationResult> ValidateFileAsync(IFormFile file);
    }

    public class InHouseFileProcessorService : IInHouseFileProcessorService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<InHouseFileProcessorService> _logger;

        public InHouseFileProcessorService(
            PepScannerDbContext context,
            ILogger<InHouseFileProcessorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WatchlistUpdateResult> ProcessFileAsync(IFormFile file, string sourceName, string? fileFormat = null)
        {
            var result = new WatchlistUpdateResult
            {
                Source = sourceName,
                ProcessingDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("Processing in-house file: {FileName} for source: {SourceName}", file.FileName, sourceName);

                // Determine file format
                var format = fileFormat ?? GetFileFormat(file.FileName);
                
                List<InHouseEntry> entries;
                
                switch (format.ToLower())
                {
                    case "csv":
                        entries = await ProcessCsvFileAsync(file);
                        break;
                    case "excel":
                    case "xlsx":
                    case "xls":
                        entries = await ProcessExcelFileAsync(file);
                        break;
                    case "json":
                        entries = await ProcessJsonFileAsync(file);
                        break;
                    default:
                        throw new NotSupportedException($"File format '{format}' is not supported");
                }

                result.TotalRecords = entries.Count;

                foreach (var entry in entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    var existingEntry = await _context.WatchlistEntries
                        .FirstOrDefaultAsync(w => w.ExternalId == entry.Id && w.Source == sourceName);

                    if (existingEntry == null)
                    {
                        var newEntry = CreateWatchlistEntryFromInHouse(entry, sourceName);
                        _context.WatchlistEntries.Add(newEntry);
                        result.NewRecords++;
                    }
                    else
                    {
                        UpdateWatchlistEntryFromInHouse(existingEntry, entry);
                        _context.WatchlistEntries.Update(existingEntry);
                        result.UpdatedRecords++;
                    }
                }

                await _context.SaveChangesAsync();
                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;

                _logger.LogInformation("In-house file processing completed. {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing in-house file: {FileName}", file.FileName);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        public async Task<List<string>> GetSupportedFormatsAsync()
        {
            return new List<string> { "CSV", "Excel", "JSON" };
        }

        public async Task<FileValidationResult> ValidateFileAsync(IFormFile file)
        {
            var result = new FileValidationResult
            {
                IsValid = true,
                FileName = file.FileName,
                FileSize = file.Length
            };

            try
            {
                // Check file size (max 50MB)
                if (file.Length > 50 * 1024 * 1024)
                {
                    result.IsValid = false;
                    result.Errors.Add("File size exceeds maximum limit of 50MB");
                }

                // Check file extension
                var format = GetFileFormat(file.FileName);
                if (string.IsNullOrEmpty(format))
                {
                    result.IsValid = false;
                    result.Errors.Add("Unsupported file format");
                }

                // Check if file is empty
                if (file.Length == 0)
                {
                    result.IsValid = false;
                    result.Errors.Add("File is empty");
                }

                // Validate file content based on format
                if (result.IsValid)
                {
                    switch (format.ToLower())
                    {
                        case "csv":
                            await ValidateCsvFileAsync(file, result);
                            break;
                        case "excel":
                        case "xlsx":
                        case "xls":
                            await ValidateExcelFileAsync(file, result);
                            break;
                        case "json":
                            await ValidateJsonFileAsync(file, result);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Validation error: {ex.Message}");
            }

            return result;
        }

        private async Task<List<InHouseEntry>> ProcessCsvFileAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<InHouseEntryMap>();
            return csv.GetRecords<InHouseEntry>().ToList();
        }

        private async Task<List<InHouseEntry>> ProcessExcelFileAsync(IFormFile file)
        {
            var entries = new List<InHouseEntry>();

            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new InvalidOperationException("No worksheet found in Excel file");

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            var colCount = worksheet.Dimension?.Columns ?? 0;

            if (rowCount < 2) // Need at least header + 1 data row
                throw new InvalidOperationException("Excel file must contain at least one data row");

            // Read headers
            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString() ?? "";
                headers.Add(header);
            }

            // Read data rows
            for (int row = 2; row <= rowCount; row++)
            {
                var entry = new InHouseEntry();
                
                for (int col = 1; col <= colCount; col++)
                {
                    var value = worksheet.Cells[row, col].Value?.ToString() ?? "";
                    var header = headers[col - 1];

                    SetPropertyValue(entry, header, value);
                }

                if (!string.IsNullOrEmpty(entry.Name))
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private async Task<List<InHouseEntry>> ProcessJsonFileAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var entries = JsonSerializer.Deserialize<List<InHouseEntry>>(jsonContent, options) ?? new List<InHouseEntry>();
            return entries.Where(e => !string.IsNullOrEmpty(e.Name)).ToList();
        }

        private async Task ValidateCsvFileAsync(IFormFile file, FileValidationResult result)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                csv.Context.RegisterClassMap<InHouseEntryMap>();
                var records = csv.GetRecords<InHouseEntry>().Take(5).ToList(); // Check first 5 records

                if (!records.Any())
                {
                    result.IsValid = false;
                    result.Errors.Add("CSV file contains no valid records");
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"CSV validation error: {ex.Message}");
            }
        }

        private async Task ValidateExcelFileAsync(IFormFile file, FileValidationResult result)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);

                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet?.Dimension == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("Excel file is empty or corrupted");
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Excel validation error: {ex.Message}");
            }
        }

        private async Task ValidateJsonFileAsync(IFormFile file, FileValidationResult result)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var jsonContent = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var entries = JsonSerializer.Deserialize<List<InHouseEntry>>(jsonContent, options);
                if (entries == null || !entries.Any())
                {
                    result.IsValid = false;
                    result.Errors.Add("JSON file contains no valid records");
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"JSON validation error: {ex.Message}");
            }
        }

        private string GetFileFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".csv" => "CSV",
                ".xlsx" => "Excel",
                ".xls" => "Excel",
                ".json" => "JSON",
                _ => ""
            };
        }

        private void SetPropertyValue(InHouseEntry entry, string header, string value)
        {
            var headerLower = header.ToLower().Trim();
            
            switch (headerLower)
            {
                case "id":
                case "entity_id":
                case "reference_id":
                    entry.Id = value;
                    break;
                case "name":
                case "full_name":
                case "entity_name":
                    entry.Name = value;
                    break;
                case "category":
                case "type":
                case "list_type":
                    entry.Category = value;
                    break;
                case "risk_level":
                case "risk":
                    entry.RiskLevel = value;
                    break;
                case "country":
                case "nationality":
                    entry.Country = value;
                    break;
                case "date_of_birth":
                case "dob":
                case "birth_date":
                    entry.DateOfBirth = value;
                    break;
                case "address":
                case "location":
                    entry.Address = value;
                    break;
                case "city":
                    entry.City = value;
                    break;
                case "state":
                case "province":
                    entry.State = value;
                    break;
                case "postal_code":
                case "zip":
                case "pin":
                    entry.PostalCode = value;
                    break;
                case "aliases":
                case "alternate_names":
                case "other_names":
                    entry.Aliases = value;
                    break;
                case "remarks":
                case "comments":
                case "notes":
                    entry.Remarks = value;
                    break;
                case "reason":
                case "sanction_reason":
                    entry.Reason = value;
                    break;
                case "designation":
                case "position":
                case "role":
                    entry.Designation = value;
                    break;
            }
        }

        private WatchlistEntry CreateWatchlistEntryFromInHouse(InHouseEntry entry, string sourceName)
        {
            return new WatchlistEntry
            {
                Id = Guid.NewGuid(),
                Source = sourceName,
                ListType = entry.Category ?? "In-House",
                PrimaryName = entry.Name ?? "",
                EntityType = "Individual",
                RiskLevel = entry.RiskLevel ?? "Medium",
                RiskCategory = entry.Category ?? "In-House",
                Country = entry.Country,
                Nationality = entry.Country,
                Citizenship = entry.Country,
                DateOfBirth = ParseDateOfBirth(entry.DateOfBirth),
                Address = entry.Address,
                City = entry.City,
                State = entry.State,
                PostalCode = entry.PostalCode,
                PositionOrRole = entry.Designation,
                PepPosition = entry.Designation,
                PepCountry = entry.Country,
                SanctionType = "Enhanced Monitoring",
                SanctionAuthority = sourceName,
                SanctionReference = entry.Id,
                SanctionReason = entry.Reason,
                ExternalId = entry.Id,
                ExternalReference = entry.Id,
                Comments = entry.Remarks,
                DateAddedUtc = DateTime.UtcNow,
                IsActive = true,
                AddedBy = "System",
                AlternateNames = entry.Aliases
            };
        }

        private void UpdateWatchlistEntryFromInHouse(WatchlistEntry existingEntry, InHouseEntry entry)
        {
            existingEntry.PrimaryName = entry.Name ?? existingEntry.PrimaryName;
            existingEntry.DateOfBirth = ParseDateOfBirth(entry.DateOfBirth) ?? existingEntry.DateOfBirth;
            existingEntry.Address = entry.Address ?? existingEntry.Address;
            existingEntry.City = entry.City ?? existingEntry.City;
            existingEntry.State = entry.State ?? existingEntry.State;
            existingEntry.PostalCode = entry.PostalCode ?? existingEntry.PostalCode;
            existingEntry.PositionOrRole = entry.Designation ?? existingEntry.PositionOrRole;
            existingEntry.SanctionReason = entry.Reason ?? existingEntry.SanctionReason;
            existingEntry.Comments = entry.Remarks ?? existingEntry.Comments;
            existingEntry.AlternateNames = entry.Aliases ?? existingEntry.AlternateNames;
            existingEntry.DateLastUpdatedUtc = DateTime.UtcNow;
            existingEntry.UpdatedBy = "System";
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

    public class InHouseEntry
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? RiskLevel { get; set; }
        public string? Country { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Aliases { get; set; }
        public string? Remarks { get; set; }
        public string? Reason { get; set; }
        public string? Designation { get; set; }
    }

    public sealed class InHouseEntryMap : CsvHelper.Configuration.ClassMap<InHouseEntry>
    {
        public InHouseEntryMap()
        {
            Map(m => m.Id).Name("ID", "Id", "id", "Entity_ID", "Reference_ID");
            Map(m => m.Name).Name("Name", "NAME", "name", "Full_Name", "Entity_Name");
            Map(m => m.Category).Name("Category", "CATEGORY", "category", "Type", "List_Type");
            Map(m => m.RiskLevel).Name("RiskLevel", "RISK_LEVEL", "risk_level", "Risk");
            Map(m => m.Country).Name("Country", "COUNTRY", "country", "Nationality");
            Map(m => m.DateOfBirth).Name("DateOfBirth", "DOB", "dob", "Date_of_Birth", "Birth_Date");
            Map(m => m.Address).Name("Address", "ADDRESS", "address", "Location");
            Map(m => m.City).Name("City", "CITY", "city");
            Map(m => m.State).Name("State", "STATE", "state", "Province");
            Map(m => m.PostalCode).Name("PostalCode", "PIN", "pin", "Postal_Code", "Zip");
            Map(m => m.Aliases).Name("Aliases", "ALIASES", "aliases", "Alternate_Names", "Other_Names");
            Map(m => m.Remarks).Name("Remarks", "REMARKS", "remarks", "Comments", "Notes");
            Map(m => m.Reason).Name("Reason", "REASON", "reason", "Sanction_Reason");
            Map(m => m.Designation).Name("Designation", "DESIGNATION", "designation", "Position", "Role");
        }
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
