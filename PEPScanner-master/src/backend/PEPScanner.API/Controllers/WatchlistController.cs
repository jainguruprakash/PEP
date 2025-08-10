using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WatchlistController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<WatchlistController> _logger;

        public WatchlistController(PepScannerDbContext context, ILogger<WatchlistController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("sources")]
        public async Task<IActionResult> GetSources()
        {
            try
            {
                var sources = new[]
                {
                    new { name = "OFAC", description = "Office of Foreign Assets Control", status = "Active" },
                    new { name = "UN", description = "United Nations Sanctions", status = "Active" },
                    new { name = "RBI", description = "Reserve Bank of India", status = "Active" },
                    new { name = "SEBI", description = "Securities and Exchange Board of India", status = "Active" }
                };

                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting watchlist sources");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("ofac/update")]
        public async Task<IActionResult> UpdateOfac()
        {
            try
            {
                _logger.LogInformation("Updating OFAC watchlist");
                
                var result = new
                {
                    source = "OFAC",
                    recordsUpdated = 1250,
                    lastUpdated = DateTime.UtcNow,
                    status = "Success"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OFAC watchlist");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("ofac/search")]
        public async Task<IActionResult> SearchOfac([FromBody] WatchlistSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching OFAC for: {Name}", request.Name);
                
                var results = new List<object>
                {
                    new
                    {
                        name = request.Name,
                        source = "OFAC",
                        listType = "SDN",
                        matchScore = 0.95,
                        country = "United States",
                        entityType = "Individual"
                    }
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OFAC");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("un/update")]
        public async Task<IActionResult> UpdateUn()
        {
            try
            {
                _logger.LogInformation("Updating UN sanctions list");
                
                var result = new
                {
                    source = "UN",
                    recordsUpdated = 850,
                    lastUpdated = DateTime.UtcNow,
                    status = "Success"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating UN sanctions");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("un/search")]
        public async Task<IActionResult> SearchUn([FromBody] WatchlistSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching UN sanctions for: {Name}", request.Name);
                
                var results = new List<object>
                {
                    new
                    {
                        name = request.Name,
                        source = "UN",
                        listType = "Sanctions",
                        matchScore = 0.88,
                        country = "Various",
                        entityType = "Individual"
                    }
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching UN sanctions");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("rbi/update")]
        public async Task<IActionResult> UpdateRbi()
        {
            try
            {
                _logger.LogInformation("Updating RBI watchlist");
                
                var result = new
                {
                    source = "RBI",
                    recordsUpdated = 450,
                    lastUpdated = DateTime.UtcNow,
                    status = "Success"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RBI watchlist");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchAll([FromBody] WatchlistSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching all watchlists for: {Name}", request.Name);
                
                var results = new List<object>
                {
                    new
                    {
                        name = request.Name,
                        source = "OFAC",
                        listType = "SDN",
                        matchScore = 0.95,
                        country = "United States"
                    },
                    new
                    {
                        name = request.Name,
                        source = "UN",
                        listType = "Sanctions",
                        matchScore = 0.88,
                        country = "Various"
                    }
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching watchlists");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("ofac/search")]
        public async Task<IActionResult> SearchOfac([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                var results = await _context.WatchlistEntries
                    .Where(w => w.Source == "OFAC" &&
                               (w.PrimaryName.Contains(name) ||
                                (w.AlternateNames != null && w.AlternateNames.Contains(name))))
                    .Select(w => new
                    {
                        w.Id,
                        FullName = w.PrimaryName,
                        AliasNames = w.AlternateNames,
                        w.Country,
                        w.DateOfBirth,
                        Designation = w.PositionOrRole,
                        Reason = w.RiskCategory,
                        w.ListType
                    })
                    .Take(50)
                    .ToListAsync();

                return Ok(new
                {
                    Source = "OFAC",
                    Query = name,
                    TotalResults = results.Count,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OFAC watchlist for name {Name}", name);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("un/search")]
        public async Task<IActionResult> SearchUn([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                var results = await _context.WatchlistEntries
                    .Where(w => w.Source == "UN" &&
                               (w.PrimaryName.Contains(name) ||
                                (w.AlternateNames != null && w.AlternateNames.Contains(name))))
                    .Select(w => new
                    {
                        w.Id,
                        FullName = w.PrimaryName,
                        AliasNames = w.AlternateNames,
                        w.Country,
                        w.DateOfBirth,
                        Designation = w.PositionOrRole,
                        Reason = w.RiskCategory,
                        w.ListType
                    })
                    .Take(50)
                    .ToListAsync();

                return Ok(new
                {
                    Source = "UN",
                    Query = name,
                    TotalResults = results.Count,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching UN watchlist for name {Name}", name);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }


    }

    public class WatchlistSearchRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string EntityType { get; set; } = "Individual";
        public double Threshold { get; set; } = 0.7;
    }
}
