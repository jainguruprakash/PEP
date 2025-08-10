using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using CsvHelper;
using System.Globalization;

namespace PEPScanner.Infrastructure.Services
{
    public interface IOrganizationCustomListService
    {
        Task<List<OrganizationCustomList>> GetCustomListsAsync(Guid organizationId);
        Task<OrganizationCustomList?> GetCustomListAsync(Guid organizationId, Guid listId);
        Task<OrganizationCustomList> CreateCustomListAsync(Guid organizationId, OrganizationCustomList customList);
        Task<OrganizationCustomList> UpdateCustomListAsync(OrganizationCustomList customList);
        Task<bool> DeleteCustomListAsync(Guid organizationId, Guid listId);
        Task<List<OrganizationCustomListEntry>> GetCustomListEntriesAsync(Guid organizationId, Guid listId, int skip = 0, int take = 100);
        Task<OrganizationCustomListEntry> AddCustomListEntryAsync(Guid organizationId, Guid listId, OrganizationCustomListEntry entry);
        Task<OrganizationCustomListEntry> UpdateCustomListEntryAsync(OrganizationCustomListEntry entry);
        Task<bool> DeleteCustomListEntryAsync(Guid organizationId, Guid listId, Guid entryId);
        Task<CustomListImportResult> ImportFromFileAsync(Guid organizationId, Guid listId, IFormFile file, string importedBy);
        Task<List<OrganizationCustomListEntry>> SearchCustomListsAsync(Guid organizationId, string searchTerm, string? listType = null);
        Task<List<OrganizationCustomListEntry>> MatchAgainstCustomListsAsync(Guid organizationId, string name, DateTime? dateOfBirth = null, string? nationality = null);
    }

    public class OrganizationCustomListService : IOrganizationCustomListService
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<OrganizationCustomListService> _logger;

        public OrganizationCustomListService(
            PepScannerDbContext context,
            ILogger<OrganizationCustomListService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<OrganizationCustomList>> GetCustomListsAsync(Guid organizationId)
        {
            return await _context.Set<OrganizationCustomList>()
                .Where(cl => cl.OrganizationId == organizationId)
                .OrderBy(cl => cl.ListName)
                .ToListAsync();
        }

        public async Task<OrganizationCustomList?> GetCustomListAsync(Guid organizationId, Guid listId)
        {
            return await _context.Set<OrganizationCustomList>()
                .Include(cl => cl.Entries)
                .FirstOrDefaultAsync(cl => cl.OrganizationId == organizationId && cl.Id == listId);
        }

        public async Task<OrganizationCustomList> CreateCustomListAsync(Guid organizationId, OrganizationCustomList customList)
        {
            customList.Id = Guid.NewGuid();
            customList.OrganizationId = organizationId;
            customList.CreatedAtUtc = DateTime.UtcNow;

            _context.Set<OrganizationCustomList>().Add(customList);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created custom list {ListName} for organization {OrganizationId}", 
                customList.ListName, organizationId);

            return customList;
        }

        public async Task<OrganizationCustomList> UpdateCustomListAsync(OrganizationCustomList customList)
        {
            customList.UpdatedAtUtc = DateTime.UtcNow;
            _context.Set<OrganizationCustomList>().Update(customList);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated custom list {ListId}", customList.Id);
            return customList;
        }

        public async Task<bool> DeleteCustomListAsync(Guid organizationId, Guid listId)
        {
            var customList = await _context.Set<OrganizationCustomList>()
                .FirstOrDefaultAsync(cl => cl.OrganizationId == organizationId && cl.Id == listId);

            if (customList == null)
                return false;

            // Delete all entries first
            var entries = await _context.Set<OrganizationCustomListEntry>()
                .Where(e => e.CustomListId == listId)
                .ToListAsync();

            _context.Set<OrganizationCustomListEntry>().RemoveRange(entries);
            _context.Set<OrganizationCustomList>().Remove(customList);
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted custom list {ListId} with {EntryCount} entries", 
                listId, entries.Count);

            return true;
        }

        public async Task<List<OrganizationCustomListEntry>> GetCustomListEntriesAsync(Guid organizationId, Guid listId, int skip = 0, int take = 100)
        {
            return await _context.Set<OrganizationCustomListEntry>()
                .Where(e => e.OrganizationId == organizationId && e.CustomListId == listId)
                .OrderBy(e => e.PrimaryName)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<OrganizationCustomListEntry> AddCustomListEntryAsync(Guid organizationId, Guid listId, OrganizationCustomListEntry entry)
        {
            entry.Id = Guid.NewGuid();
            entry.OrganizationId = organizationId;
            entry.CustomListId = listId;
            entry.DateAddedUtc = DateTime.UtcNow;

            _context.Set<OrganizationCustomListEntry>().Add(entry);
            await _context.SaveChangesAsync();

            // Update list statistics
            await UpdateListStatisticsAsync(listId);

            _logger.LogInformation("Added entry {EntryName} to custom list {ListId}", 
                entry.PrimaryName, listId);

            return entry;
        }

        public async Task<OrganizationCustomListEntry> UpdateCustomListEntryAsync(OrganizationCustomListEntry entry)
        {
            entry.LastUpdatedUtc = DateTime.UtcNow;
            _context.Set<OrganizationCustomListEntry>().Update(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated custom list entry {EntryId}", entry.Id);
            return entry;
        }

        public async Task<bool> DeleteCustomListEntryAsync(Guid organizationId, Guid listId, Guid entryId)
        {
            var entry = await _context.Set<OrganizationCustomListEntry>()
                .FirstOrDefaultAsync(e => e.OrganizationId == organizationId && 
                                         e.CustomListId == listId && 
                                         e.Id == entryId);

            if (entry == null)
                return false;

            _context.Set<OrganizationCustomListEntry>().Remove(entry);
            await _context.SaveChangesAsync();

            // Update list statistics
            await UpdateListStatisticsAsync(listId);

            _logger.LogInformation("Deleted custom list entry {EntryId}", entryId);
            return true;
        }

        public async Task<CustomListImportResult> ImportFromFileAsync(Guid organizationId, Guid listId, IFormFile file, string importedBy)
        {
            var result = new CustomListImportResult
            {
                ListId = listId,
                ImportedBy = importedBy,
                ImportedAt = DateTime.UtcNow
            };

            try
            {
                var entries = new List<OrganizationCustomListEntry>();

                if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    entries = await ParseCsvFileAsync(file, organizationId, listId, importedBy);
                }
                else if (file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    entries = await ParseJsonFileAsync(file, organizationId, listId, importedBy);
                }
                else
                {
                    throw new NotSupportedException($"File format not supported: {file.FileName}");
                }

                // Add entries to database
                _context.Set<OrganizationCustomListEntry>().AddRange(entries);
                await _context.SaveChangesAsync();

                // Update list statistics
                await UpdateListStatisticsAsync(listId);

                result.Success = true;
                result.TotalRecords = entries.Count;
                result.ImportedRecords = entries.Count;

                _logger.LogInformation("Successfully imported {Count} entries to custom list {ListId}", 
                    entries.Count, listId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error importing file to custom list {ListId}", listId);
            }

            return result;
        }

        public async Task<List<OrganizationCustomListEntry>> SearchCustomListsAsync(Guid organizationId, string searchTerm, string? listType = null)
        {
            var query = _context.Set<OrganizationCustomListEntry>()
                .Where(e => e.OrganizationId == organizationId && e.IsActive);

            if (!string.IsNullOrEmpty(listType))
            {
                query = query.Where(e => e.EntryType == listType);
            }

            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(e => 
                e.PrimaryName.ToLower().Contains(searchLower) ||
                e.AlternateNames.ToLower().Contains(searchLower) ||
                e.FirstName.ToLower().Contains(searchLower) ||
                e.LastName.ToLower().Contains(searchLower));

            return await query
                .OrderBy(e => e.PrimaryName)
                .Take(100)
                .ToListAsync();
        }

        public async Task<List<OrganizationCustomListEntry>> MatchAgainstCustomListsAsync(Guid organizationId, string name, DateTime? dateOfBirth = null, string? nationality = null)
        {
            var query = _context.Set<OrganizationCustomListEntry>()
                .Where(e => e.OrganizationId == organizationId && e.IsActive);

            var nameLower = name.ToLowerInvariant();
            query = query.Where(e => 
                e.PrimaryName.ToLower().Contains(nameLower) ||
                e.AlternateNames.ToLower().Contains(nameLower) ||
                (e.FirstName.ToLower() + " " + e.LastName.ToLower()).Contains(nameLower));

            if (dateOfBirth.HasValue)
            {
                query = query.Where(e => e.DateOfBirth.HasValue && 
                                        e.DateOfBirth.Value.Date == dateOfBirth.Value.Date);
            }

            if (!string.IsNullOrEmpty(nationality))
            {
                var nationalityLower = nationality.ToLowerInvariant();
                query = query.Where(e => e.Nationality.ToLower().Contains(nationalityLower));
            }

            return await query
                .OrderBy(e => e.PrimaryName)
                .Take(50)
                .ToListAsync();
        }

        private async Task UpdateListStatisticsAsync(Guid listId)
        {
            var customList = await _context.Set<OrganizationCustomList>()
                .FirstOrDefaultAsync(cl => cl.Id == listId);

            if (customList != null)
            {
                var totalEntries = await _context.Set<OrganizationCustomListEntry>()
                    .CountAsync(e => e.CustomListId == listId);

                var activeEntries = await _context.Set<OrganizationCustomListEntry>()
                    .CountAsync(e => e.CustomListId == listId && e.IsActive);

                customList.TotalEntries = totalEntries;
                customList.ActiveEntries = activeEntries;
                customList.LastUpdateAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<OrganizationCustomListEntry>> ParseCsvFileAsync(IFormFile file, Guid organizationId, Guid listId, string importedBy)
        {
            var entries = new List<OrganizationCustomListEntry>();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                var entry = new OrganizationCustomListEntry
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    CustomListId = listId,
                    PrimaryName = csv.GetField("Name") ?? csv.GetField("PrimaryName") ?? "",
                    FirstName = csv.GetField("FirstName") ?? "",
                    LastName = csv.GetField("LastName") ?? "",
                    DateOfBirth = ParseDate(csv.GetField("DateOfBirth")),
                    Nationality = csv.GetField("Nationality") ?? "",
                    Country = csv.GetField("Country") ?? "",
                    PositionOrRole = csv.GetField("Position") ?? csv.GetField("Role") ?? "",
                    EntryType = csv.GetField("Type") ?? "PEP",
                    RiskCategory = csv.GetField("RiskLevel") ?? "Medium",
                    DateAddedUtc = DateTime.UtcNow,
                    AddedBy = importedBy,
                    Source = "CSV Import",
                    SourceReference = file.FileName
                };

                entries.Add(entry);
            }

            return entries;
        }

        private async Task<List<OrganizationCustomListEntry>> ParseJsonFileAsync(IFormFile file, Guid organizationId, Guid listId, string importedBy)
        {
            using var stream = file.OpenReadStream();
            var jsonString = await new StreamReader(stream).ReadToEndAsync();
            var jsonEntries = JsonSerializer.Deserialize<List<JsonElement>>(jsonString) ?? new List<JsonElement>();

            var entries = new List<OrganizationCustomListEntry>();

            foreach (var jsonEntry in jsonEntries)
            {
                var entry = new OrganizationCustomListEntry
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    CustomListId = listId,
                    PrimaryName = GetJsonProperty(jsonEntry, "name") ?? GetJsonProperty(jsonEntry, "primaryName") ?? "",
                    FirstName = GetJsonProperty(jsonEntry, "firstName") ?? "",
                    LastName = GetJsonProperty(jsonEntry, "lastName") ?? "",
                    DateOfBirth = ParseDate(GetJsonProperty(jsonEntry, "dateOfBirth")),
                    Nationality = GetJsonProperty(jsonEntry, "nationality") ?? "",
                    Country = GetJsonProperty(jsonEntry, "country") ?? "",
                    PositionOrRole = GetJsonProperty(jsonEntry, "position") ?? GetJsonProperty(jsonEntry, "role") ?? "",
                    EntryType = GetJsonProperty(jsonEntry, "type") ?? "PEP",
                    RiskCategory = GetJsonProperty(jsonEntry, "riskLevel") ?? "Medium",
                    DateAddedUtc = DateTime.UtcNow,
                    AddedBy = importedBy,
                    Source = "JSON Import",
                    SourceReference = file.FileName
                };

                entries.Add(entry);
            }

            return entries;
        }

        private string? GetJsonProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.GetString();
            }
            return null;
        }

        private DateTime? ParseDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            if (DateTime.TryParse(dateString, out var date))
                return date;

            return null;
        }
    }

    public class CustomListImportResult
    {
        public Guid ListId { get; set; }
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ImportedAt { get; set; }
        public string ImportedBy { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
