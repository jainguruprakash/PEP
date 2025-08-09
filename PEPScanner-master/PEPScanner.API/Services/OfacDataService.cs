using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.API.Models;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace PEPScanner.API.Services
{
    public class OfacDataService : IOfacDataService
    {
        private readonly HttpClient _httpClient;
        private readonly PepScannerDbContext _context;
        private readonly ILogger<OfacDataService> _logger;
        private readonly IConfiguration _configuration;

        // OFAC API endpoints
        private const string OFAC_SDN_URL = "https://www.treasury.gov/ofac/downloads/sdn.csv";
        private const string OFAC_CONSOLIDATED_URL = "https://www.treasury.gov/ofac/downloads/consolidated/consolidated.csv";
        private const string OFAC_ADDITIONS_URL = "https://www.treasury.gov/ofac/downloads/add.csv";
        private const string OFAC_DELETIONS_URL = "https://www.treasury.gov/ofac/downloads/del.csv";

        public OfacDataService(
            HttpClient httpClient,
            PepScannerDbContext context,
            ILogger<OfacDataService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<OfacEntry>> FetchSdnListAsync()
        {
            try
            {
                _logger.LogInformation("Fetching OFAC SDN list from {Url}", OFAC_SDN_URL);
                
                var response = await _httpClient.GetAsync(OFAC_SDN_URL);
                response.EnsureSuccessStatusCode();
                
                var csvContent = await response.Content.ReadAsStringAsync();
                return ParseOfacCsv(csvContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching OFAC SDN list");
                throw;
            }
        }

        public async Task<List<OfacEntry>> SearchByNameAsync(string name)
        {
            try
            {
                var allEntries = await FetchSdnListAsync();
                return allEntries.Where(e => 
                    e.Name?.Contains(name, StringComparison.OrdinalIgnoreCase) == true ||
                    e.Title?.Contains(name, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OFAC data for name: {Name}", name);
                throw;
            }
        }

        public async Task<int> UpdateWatchlistFromOfacAsync()
        {
            try
            {
                _logger.LogInformation("Starting OFAC watchlist update");
                
                var ofacEntries = await FetchSdnListAsync();
                var updatedCount = 0;

                foreach (var ofacEntry in ofacEntries)
                {
                    if (string.IsNullOrEmpty(ofacEntry.Name))
                        continue;

                    var existingEntry = await _context.WatchlistEntries
                        .FirstOrDefaultAsync(w => w.ExternalId == ofacEntry.EntityNumber && w.Source == "OFAC");

                    if (existingEntry == null)
                    {
                        // Create new watchlist entry
                        var newEntry = new WatchlistEntry
                        {
                            Id = Guid.NewGuid(),
                            Source = "OFAC",
                            ListType = "Sanctions",
                            PrimaryName = ofacEntry.Name,
                            EntityType = GetEntityType(ofacEntry.SdnType),
                            RiskLevel = "High",
                            RiskCategory = "Sanctions",
                            Country = ofacEntry.Country,
                            Nationality = ofacEntry.Nationality,
                            Citizenship = ofacEntry.Citizenship,
                            DateOfBirth = ParseDateOfBirth(ofacEntry.DateOfBirth),
                            Address = ofacEntry.Address,
                            City = ofacEntry.City,
                            State = ofacEntry.State,
                            PostalCode = ofacEntry.PostalCode,
                            PositionOrRole = ofacEntry.Title,
                            PepPosition = ofacEntry.Title,
                            PepCountry = ofacEntry.Country,
                            SanctionType = "Asset Freeze",
                            SanctionAuthority = "OFAC",
                            SanctionReference = ofacEntry.EntityNumber,
                            SanctionReason = ofacEntry.Program,
                            ExternalId = ofacEntry.EntityNumber,
                            ExternalReference = ofacEntry.EntityNumber,
                            Comments = ofacEntry.Remarks,
                            DateAddedUtc = DateTime.UtcNow,
                            IsActive = true,
                            AddedBy = "System"
                        };

                        // Add alternate names/identifications
                        var alternateNames = new List<string>();
                        if (!string.IsNullOrEmpty(ofacEntry.IdNumber)) alternateNames.Add($"ID: {ofacEntry.IdNumber}");
                        if (!string.IsNullOrEmpty(ofacEntry.IdNumber2)) alternateNames.Add($"ID2: {ofacEntry.IdNumber2}");
                        if (!string.IsNullOrEmpty(ofacEntry.IdNumber3)) alternateNames.Add($"ID3: {ofacEntry.IdNumber3}");
                        if (!string.IsNullOrEmpty(ofacEntry.CallSign)) alternateNames.Add($"Call Sign: {ofacEntry.CallSign}");
                        
                        newEntry.AlternateNames = string.Join("; ", alternateNames);

                        _context.WatchlistEntries.Add(newEntry);
                        updatedCount++;
                    }
                    else
                    {
                        // Update existing entry
                        existingEntry.PrimaryName = ofacEntry.Name;
                        existingEntry.Country = ofacEntry.Country;
                        existingEntry.Nationality = ofacEntry.Nationality;
                        existingEntry.Citizenship = ofacEntry.Citizenship;
                        existingEntry.DateOfBirth = ParseDateOfBirth(ofacEntry.DateOfBirth);
                        existingEntry.Address = ofacEntry.Address;
                        existingEntry.City = ofacEntry.City;
                        existingEntry.State = ofacEntry.State;
                        existingEntry.PostalCode = ofacEntry.PostalCode;
                        existingEntry.PositionOrRole = ofacEntry.Title;
                        existingEntry.SanctionReason = ofacEntry.Program;
                        existingEntry.Comments = ofacEntry.Remarks;
                        existingEntry.DateLastUpdatedUtc = DateTime.UtcNow;
                        existingEntry.UpdatedBy = "System";

                        _context.WatchlistEntries.Update(existingEntry);
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("OFAC watchlist update completed. {Count} entries processed", updatedCount);
                
                return updatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating watchlist from OFAC");
                throw;
            }
        }

        public async Task<OfacProcessingResult> ProcessOfacFileAsync(string fileType = "sdn")
        {
            var result = new OfacProcessingResult();
            
            try
            {
                _logger.LogInformation("Processing OFAC file: {FileType}", fileType);
                
                string url = fileType.ToLower() switch
                {
                    "sdn" => OFAC_SDN_URL,
                    "cons" => OFAC_CONSOLIDATED_URL,
                    "add" => OFAC_ADDITIONS_URL,
                    "del" => OFAC_DELETIONS_URL,
                    _ => OFAC_SDN_URL
                };

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var csvContent = await response.Content.ReadAsStringAsync();
                var ofacEntries = ParseOfacCsv(csvContent);
                
                result.TotalRecords = ofacEntries.Count;
                result.Success = true;
                
                // Process entries based on file type
                if (fileType.ToLower() == "del")
                {
                    // Handle deletions
                    foreach (var entry in ofacEntries)
                    {
                        var existingEntry = await _context.WatchlistEntries
                            .FirstOrDefaultAsync(w => w.ExternalId == entry.EntityNumber && w.Source == "OFAC");
                        
                        if (existingEntry != null)
                        {
                            existingEntry.IsActive = false;
                            existingEntry.DateRemovedUtc = DateTime.UtcNow;
                            existingEntry.RemovedBy = "System";
                            result.DeletedRecords++;
                        }
                    }
                }
                else
                {
                    // Handle additions/updates
                    foreach (var entry in ofacEntries)
                    {
                        var existingEntry = await _context.WatchlistEntries
                            .FirstOrDefaultAsync(w => w.ExternalId == entry.EntityNumber && w.Source == "OFAC");
                        
                        if (existingEntry == null)
                        {
                            // Create new entry
                            var newEntry = CreateWatchlistEntryFromOfac(entry);
                            _context.WatchlistEntries.Add(newEntry);
                            result.NewRecords++;
                        }
                        else
                        {
                            // Update existing entry
                            UpdateWatchlistEntryFromOfac(existingEntry, entry);
                            _context.WatchlistEntries.Update(existingEntry);
                            result.UpdatedRecords++;
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("OFAC file processing completed: {Result}", result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OFAC file: {FileType}", fileType);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<DateTime?> GetLastUpdateTimestampAsync()
        {
            try
            {
                var lastEntry = await _context.WatchlistEntries
                    .Where(w => w.Source == "OFAC")
                    .OrderByDescending(w => w.DateLastUpdatedUtc)
                    .FirstOrDefaultAsync();
                
                return lastEntry?.DateLastUpdatedUtc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last OFAC update timestamp");
                return null;
            }
        }

        private List<OfacEntry> ParseOfacCsv(string csvContent)
        {
            var entries = new List<OfacEntry>();
            
            try
            {
                using var reader = new StringReader(csvContent);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                
                // Configure CSV reader for OFAC format
                csv.Context.RegisterClassMap<OfacEntryMap>();
                
                entries = csv.GetRecords<OfacEntry>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing OFAC CSV content");
                throw;
            }
            
            return entries;
        }

        private string GetEntityType(string? sdnType)
        {
            return sdnType?.ToLower() switch
            {
                "individual" => "Individual",
                "entity" => "Organization",
                "vessel" => "Vessel",
                "aircraft" => "Aircraft",
                _ => "Individual"
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

        private WatchlistEntry CreateWatchlistEntryFromOfac(OfacEntry ofacEntry)
        {
            return new WatchlistEntry
            {
                Id = Guid.NewGuid(),
                Source = "OFAC",
                ListType = "Sanctions",
                PrimaryName = ofacEntry.Name ?? "",
                EntityType = GetEntityType(ofacEntry.SdnType),
                RiskLevel = "High",
                RiskCategory = "Sanctions",
                Country = ofacEntry.Country,
                Nationality = ofacEntry.Nationality,
                Citizenship = ofacEntry.Citizenship,
                DateOfBirth = ParseDateOfBirth(ofacEntry.DateOfBirth),
                Address = ofacEntry.Address,
                City = ofacEntry.City,
                State = ofacEntry.State,
                PostalCode = ofacEntry.PostalCode,
                PositionOrRole = ofacEntry.Title,
                PepPosition = ofacEntry.Title,
                PepCountry = ofacEntry.Country,
                SanctionType = "Asset Freeze",
                SanctionAuthority = "OFAC",
                SanctionReference = ofacEntry.EntityNumber,
                SanctionReason = ofacEntry.Program,
                ExternalId = ofacEntry.EntityNumber,
                ExternalReference = ofacEntry.EntityNumber,
                Comments = ofacEntry.Remarks,
                DateAddedUtc = DateTime.UtcNow,
                IsActive = true,
                AddedBy = "System"
            };
        }

        private void UpdateWatchlistEntryFromOfac(WatchlistEntry existingEntry, OfacEntry ofacEntry)
        {
            existingEntry.PrimaryName = ofacEntry.Name ?? existingEntry.PrimaryName;
            existingEntry.Country = ofacEntry.Country ?? existingEntry.Country;
            existingEntry.Nationality = ofacEntry.Nationality ?? existingEntry.Nationality;
            existingEntry.Citizenship = ofacEntry.Citizenship ?? existingEntry.Citizenship;
            existingEntry.DateOfBirth = ParseDateOfBirth(ofacEntry.DateOfBirth) ?? existingEntry.DateOfBirth;
            existingEntry.Address = ofacEntry.Address ?? existingEntry.Address;
            existingEntry.City = ofacEntry.City ?? existingEntry.City;
            existingEntry.State = ofacEntry.State ?? existingEntry.State;
            existingEntry.PostalCode = ofacEntry.PostalCode ?? existingEntry.PostalCode;
            existingEntry.PositionOrRole = ofacEntry.Title ?? existingEntry.PositionOrRole;
            existingEntry.SanctionReason = ofacEntry.Program ?? existingEntry.SanctionReason;
            existingEntry.Comments = ofacEntry.Remarks ?? existingEntry.Comments;
            existingEntry.DateLastUpdatedUtc = DateTime.UtcNow;
            existingEntry.UpdatedBy = "System";
        }
    }

    // CSV mapping for OFAC data
    public sealed class OfacEntryMap : CsvHelper.Configuration.ClassMap<OfacEntry>
    {
        public OfacEntryMap()
        {
            Map(m => m.EntityNumber).Name("ent_num");
            Map(m => m.SdnType).Name("sdn_type");
            Map(m => m.Program).Name("program");
            Map(m => m.Name).Name("sdn_name");
            Map(m => m.Title).Name("title");
            Map(m => m.CallSign).Name("call_sign");
            Map(m => m.VesselType).Name("vess_type");
            Map(m => m.Tonnage).Name("tonnage");
            Map(m => m.GrossRegisteredTonnage).Name("grt");
            Map(m => m.VesselFlag).Name("vess_flag");
            Map(m => m.VesselOwner).Name("vess_owner");
            Map(m => m.Remarks).Name("remarks");
            Map(m => m.SdnDate).Name("sdn_date");
            Map(m => m.Citizenship).Name("citizenship");
            Map(m => m.Nationality).Name("nationality");
            Map(m => m.DateOfBirth).Name("date_of_birth");
            Map(m => m.PlaceOfBirth).Name("place_of_birth");
            Map(m => m.Address).Name("address");
            Map(m => m.City).Name("city");
            Map(m => m.State).Name("state");
            Map(m => m.PostalCode).Name("postal_code");
            Map(m => m.Country).Name("country");
            Map(m => m.IdNumber).Name("id_number");
            Map(m => m.IdType).Name("id_type");
            Map(m => m.IdCountry).Name("id_country");
            Map(m => m.IdExpiration).Name("id_expiration");
            Map(m => m.IdNumber2).Name("id_number2");
            Map(m => m.IdType2).Name("id_type2");
            Map(m => m.IdCountry2).Name("id_country2");
            Map(m => m.IdExpiration2).Name("id_expiration2");
            Map(m => m.IdNumber3).Name("id_number3");
            Map(m => m.IdType3).Name("id_type3");
            Map(m => m.IdCountry3).Name("id_country3");
            Map(m => m.IdExpiration3).Name("id_expiration3");
            Map(m => m.IdNumber4).Name("id_number4");
            Map(m => m.IdType4).Name("id_type4");
            Map(m => m.IdCountry4).Name("id_country4");
            Map(m => m.IdExpiration4).Name("id_expiration4");
            Map(m => m.IdNumber5).Name("id_number5");
            Map(m => m.IdType5).Name("id_type5");
            Map(m => m.IdCountry5).Name("id_country5");
            Map(m => m.IdExpiration5).Name("id_expiration5");
            Map(m => m.IdNumber6).Name("id_number6");
            Map(m => m.IdType6).Name("id_type6");
            Map(m => m.IdCountry6).Name("id_country6");
            Map(m => m.IdExpiration6).Name("id_expiration6");
            Map(m => m.IdNumber7).Name("id_number7");
            Map(m => m.IdType7).Name("id_type7");
            Map(m => m.IdCountry7).Name("id_country7");
            Map(m => m.IdExpiration7).Name("id_expiration7");
            Map(m => m.IdNumber8).Name("id_number8");
            Map(m => m.IdType8).Name("id_type8");
            Map(m => m.IdCountry8).Name("id_country8");
            Map(m => m.IdExpiration8).Name("id_expiration8");
            Map(m => m.IdNumber9).Name("id_number9");
            Map(m => m.IdType9).Name("id_type9");
            Map(m => m.IdCountry9).Name("id_country9");
            Map(m => m.IdExpiration9).Name("id_expiration9");
            Map(m => m.IdNumber10).Name("id_number10");
            Map(m => m.IdType10).Name("id_type10");
            Map(m => m.IdCountry10).Name("id_country10");
            Map(m => m.IdExpiration10).Name("id_expiration10");
        }
    }
}
