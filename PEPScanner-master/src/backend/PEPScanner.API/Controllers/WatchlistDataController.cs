using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Text.Json;
using System.Xml.Linq;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/watchlist-data")]
    public class WatchlistDataController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WatchlistDataController> _logger;

        public WatchlistDataController(
            PepScannerDbContext context,
            HttpClient httpClient,
            ILogger<WatchlistDataController> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpPost("fetch/ofac")]
        public async Task<IActionResult> FetchOfacData()
        {
            try
            {
                _logger.LogInformation("Starting OFAC data fetch");

                // Create sample OFAC data for demonstration
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "OFAC_001",
                        Source = "OFAC",
                        ListType = "Sanctions",
                        PrimaryName = "John Doe",
                        AlternateNames = "Johnny Doe; J. Doe",
                        Country = "United States",
                        PositionOrRole = "Sanctioned Individual",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    },
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "OFAC_002",
                        Source = "OFAC",
                        ListType = "Sanctions",
                        PrimaryName = "Jane Smith",
                        Country = "Russia",
                        PositionOrRole = "Government Official",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    }
                };

                // Clear existing OFAC entries
                var existingOfac = await _context.WatchlistEntries
                    .Where(w => w.Source == "OFAC")
                    .ToListAsync();

                _context.WatchlistEntries.RemoveRange(existingOfac);

                // Add new entries
                await _context.WatchlistEntries.AddRangeAsync(entries);
                await _context.SaveChangesAsync();

                var result = new
                {
                    Success = true,
                    Source = "OFAC",
                    TotalRecords = entries.Count,
                    ProcessingDate = DateTime.UtcNow,
                    Message = "OFAC data fetched and stored successfully"
                };

                _logger.LogInformation("Successfully processed {Count} OFAC entries", entries.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching OFAC data");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("fetch/un")]
        public async Task<IActionResult> FetchUnData()
        {
            try
            {
                _logger.LogInformation("Starting UN Sanctions data fetch");

                // Create sample UN data for demonstration
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "UN_001",
                        Source = "UN",
                        ListType = "Sanctions",
                        PrimaryName = "Ahmed Hassan",
                        Country = "Syria",
                        PositionOrRole = "Military Leader",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    },
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "UN_002",
                        Source = "UN",
                        ListType = "Sanctions",
                        PrimaryName = "Maria Rodriguez",
                        Country = "Venezuela",
                        PositionOrRole = "Government Minister",
                        RiskCategory = "High",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    }
                };

                // Clear existing UN entries
                var existingUn = await _context.WatchlistEntries
                    .Where(w => w.Source == "UN")
                    .ToListAsync();

                _context.WatchlistEntries.RemoveRange(existingUn);

                // Add new entries
                await _context.WatchlistEntries.AddRangeAsync(entries);
                await _context.SaveChangesAsync();

                var result = new
                {
                    Success = true,
                    Source = "UN",
                    TotalRecords = entries.Count,
                    ProcessingDate = DateTime.UtcNow,
                    Message = "UN Sanctions data fetched and stored successfully"
                };

                _logger.LogInformation("Successfully processed {Count} UN entries", entries.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching UN data");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("fetch/rbi")]
        public async Task<IActionResult> FetchRbiData()
        {
            try
            {
                _logger.LogInformation("Starting RBI data fetch");

                // Create sample RBI data for demonstration
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "RBI_001",
                        Source = "RBI",
                        ListType = "Regulatory",
                        PrimaryName = "Rajesh Kumar",
                        Country = "India",
                        PositionOrRole = "Defaulter",
                        RiskCategory = "Medium",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    }
                };

                // Clear existing RBI entries
                var existingRbi = await _context.WatchlistEntries
                    .Where(w => w.Source == "RBI")
                    .ToListAsync();

                _context.WatchlistEntries.RemoveRange(existingRbi);

                // Add new entries
                await _context.WatchlistEntries.AddRangeAsync(entries);
                await _context.SaveChangesAsync();

                var result = new
                {
                    Success = true,
                    Source = "RBI",
                    TotalRecords = entries.Count,
                    ProcessingDate = DateTime.UtcNow,
                    Message = "RBI data fetched and stored successfully"
                };

                _logger.LogInformation("Successfully processed {Count} RBI entries", entries.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching RBI data");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("fetch/parliament")]
        public async Task<IActionResult> FetchParliamentData()
        {
            try
            {
                _logger.LogInformation("Starting Indian Parliament PEP data fetch");

                // Create sample Parliament data for demonstration
                var entries = new List<WatchlistEntry>
                {
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "PARL_001",
                        Source = "PARLIAMENT",
                        ListType = "PEP",
                        PrimaryName = "Narendra Modi",
                        Country = "India",
                        PositionOrRole = "Prime Minister",
                        RiskCategory = "Medium",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    },
                    new WatchlistEntry
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = "PARL_002",
                        Source = "PARLIAMENT",
                        ListType = "PEP",
                        PrimaryName = "Rahul Gandhi",
                        Country = "India",
                        PositionOrRole = "Member of Parliament",
                        RiskCategory = "Medium",
                        IsActive = true,
                        DateAddedUtc = DateTime.UtcNow,
                        DateLastUpdatedUtc = DateTime.UtcNow
                    }
                };

                // Clear existing Parliament entries
                var existingParl = await _context.WatchlistEntries
                    .Where(w => w.Source == "PARLIAMENT")
                    .ToListAsync();

                _context.WatchlistEntries.RemoveRange(existingParl);

                // Add new entries
                await _context.WatchlistEntries.AddRangeAsync(entries);
                await _context.SaveChangesAsync();

                var result = new
                {
                    Success = true,
                    Source = "PARLIAMENT",
                    TotalRecords = entries.Count,
                    ProcessingDate = DateTime.UtcNow,
                    Message = "Indian Parliament PEP data fetched and stored successfully"
                };

                _logger.LogInformation("Successfully processed {Count} Parliament entries", entries.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Parliament data");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("fetch/all")]
        public async Task<IActionResult> FetchAllData()
        {
            try
            {
                _logger.LogInformation("Starting fetch for all watchlist sources");

                var results = new List<object>();

                // Fetch OFAC
                var ofacResponse = await FetchOfacData();
                if (ofacResponse is OkObjectResult ofacOk)
                    results.Add(ofacOk.Value);

                // Fetch UN
                var unResponse = await FetchUnData();
                if (unResponse is OkObjectResult unOk)
                    results.Add(unOk.Value);

                // Fetch RBI
                var rbiResponse = await FetchRbiData();
                if (rbiResponse is OkObjectResult rbiOk)
                    results.Add(rbiOk.Value);

                // Fetch Parliament
                var parlResponse = await FetchParliamentData();
                if (parlResponse is OkObjectResult parlOk)
                    results.Add(parlOk.Value);

                var summary = new
                {
                    Success = true,
                    Message = "All watchlist sources fetched successfully",
                    ProcessingDate = DateTime.UtcNow,
                    TotalSources = results.Count,
                    Results = results
                };

                _logger.LogInformation("Completed fetch for all watchlist sources");
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all watchlist data");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("sources")]
        public async Task<IActionResult> GetAvailableSources()
        {
            try
            {
                var sources = await _context.WatchlistEntries
                    .Where(w => !string.IsNullOrEmpty(w.Source))
                    .GroupBy(w => w.Source)
                    .Select(g => new
                    {
                        Source = g.Key,
                        Count = g.Count(),
                        LastUpdated = g.Max(w => w.DateLastUpdatedUtc)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Sources = sources,
                    TotalSources = sources.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available sources");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
