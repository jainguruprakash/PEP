using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEPScanner.API.Services;
using PEPScanner.API.Models;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GenericWatchlistController : ControllerBase
    {
        private readonly IWatchlistServiceRegistry _registry;
        private readonly ILogger<GenericWatchlistController> _logger;

        public GenericWatchlistController(
            IWatchlistServiceRegistry registry,
            ILogger<GenericWatchlistController> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        /// <summary>
        /// Get all available watchlist sources
        /// </summary>
        [HttpGet("sources")]
        [Authorize(Policy = "ComplianceOfficer")]
        public ActionResult<List<WatchlistSource>> GetAllSources()
        {
            try
            {
                var sources = _registry.GetAllSourceConfigurations();
                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all watchlist sources");
                return StatusCode(500, new { error = "Failed to get watchlist sources", details = ex.Message });
            }
        }

        /// <summary>
        /// Get watchlist sources by type (Global, Local, InHouse)
        /// </summary>
        [HttpGet("sources/by-type/{type}")]
        [Authorize(Policy = "ComplianceOfficer")]
        public ActionResult<List<WatchlistSource>> GetSourcesByType(string type)
        {
            try
            {
                var services = _registry.GetServicesByType(type);
                var sources = services.Select(s => s.GetSourceConfiguration()).ToList();
                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting watchlist sources by type: {Type}", type);
                return StatusCode(500, new { error = "Failed to get watchlist sources by type", details = ex.Message });
            }
        }

        /// <summary>
        /// Get watchlist sources by country
        /// </summary>
        [HttpGet("sources/by-country/{country}")]
        [Authorize(Policy = "ComplianceOfficer")]
        public ActionResult<List<WatchlistSource>> GetSourcesByCountry(string country)
        {
            try
            {
                var services = _registry.GetServicesByCountry(country);
                var sources = services.Select(s => s.GetSourceConfiguration()).ToList();
                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting watchlist sources by country: {Country}", country);
                return StatusCode(500, new { error = "Failed to get watchlist sources by country", details = ex.Message });
            }
        }

        /// <summary>
        /// Update a specific watchlist by source name
        /// </summary>
        [HttpPost("update/{sourceName}")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> UpdateWatchlist(string sourceName)
        {
            try
            {
                var service = _registry.GetService(sourceName);
                if (service == null)
                {
                    return NotFound(new { error = $"Watchlist service '{sourceName}' not found" });
                }

                _logger.LogInformation("Starting watchlist update for source: {SourceName}", sourceName);
                var result = await service.UpdateWatchlistAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating watchlist for source: {SourceName}", sourceName);
                return StatusCode(500, new { error = "Failed to update watchlist", details = ex.Message });
            }
        }

        /// <summary>
        /// Update all watchlists
        /// </summary>
        [HttpPost("update-all")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<List<WatchlistUpdateResult>>> UpdateAllWatchlists()
        {
            try
            {
                _logger.LogInformation("Starting update of all watchlists");
                
                var services = _registry.GetActiveServices();
                var results = new List<WatchlistUpdateResult>();

                foreach (var service in services)
                {
                    try
                    {
                        var result = await service.UpdateWatchlistAsync();
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating watchlist for source: {SourceName}", service.SourceName);
                        results.Add(new WatchlistUpdateResult
                        {
                            Source = service.SourceName,
                            Success = false,
                            ErrorMessage = ex.Message,
                            ProcessingDate = DateTime.UtcNow
                        });
                    }
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all watchlists");
                return StatusCode(500, new { error = "Failed to update all watchlists", details = ex.Message });
            }
        }

        /// <summary>
        /// Search across all watchlists by name
        /// </summary>
        [HttpGet("search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> SearchAllWatchlists([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                _logger.LogInformation("Searching all watchlists for name: {Name}", name);
                
                var services = _registry.GetActiveServices();
                var results = new Dictionary<string, object>();

                foreach (var service in services)
                {
                    try
                    {
                        var entries = await service.SearchByNameAsync(name);
                        results[service.SourceName] = new
                        {
                            Count = entries.Count,
                            Entries = entries.Take(10).Select(e => new
                            {
                                e.PrimaryName,
                                e.RiskLevel,
                                e.RiskCategory,
                                e.Country,
                                e.PositionOrRole
                            })
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error searching watchlist for source: {SourceName}", service.SourceName);
                        results[service.SourceName] = new { Error = ex.Message };
                    }
                }

                return Ok(new
                {
                    SearchTerm = name,
                    Timestamp = DateTime.UtcNow,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching all watchlists for name: {Name}", name);
                return StatusCode(500, new { error = "Failed to search watchlists", details = ex.Message });
            }
        }

        /// <summary>
        /// Search a specific watchlist by name
        /// </summary>
        [HttpGet("search/{sourceName}")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<WatchlistEntry>>> SearchWatchlist(string sourceName, [FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest(new { error = "Name parameter is required" });
                }

                var service = _registry.GetService(sourceName);
                if (service == null)
                {
                    return NotFound(new { error = $"Watchlist service '{sourceName}' not found" });
                }

                _logger.LogInformation("Searching watchlist {SourceName} for name: {Name}", sourceName, name);
                var entries = await service.SearchByNameAsync(name);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching watchlist {SourceName} for name: {Name}", sourceName, name);
                return StatusCode(500, new { error = "Failed to search watchlist", details = ex.Message });
            }
        }

        /// <summary>
        /// Process file upload for a specific watchlist
        /// </summary>
        [HttpPost("upload/{sourceName}")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<WatchlistUpdateResult>> UploadFile(string sourceName, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "File is required" });
                }

                var service = _registry.GetService(sourceName);
                if (service == null)
                {
                    return NotFound(new { error = $"Watchlist service '{sourceName}' not found" });
                }

                _logger.LogInformation("Processing file upload for source: {SourceName}, file: {FileName}", sourceName, file.FileName);
                var result = await service.ProcessFileAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file upload for source: {SourceName}", sourceName);
                return StatusCode(500, new { error = "Failed to process file upload", details = ex.Message });
            }
        }

        /// <summary>
        /// Get last update timestamp for a specific watchlist
        /// </summary>
        [HttpGet("last-update/{sourceName}")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> GetLastUpdateTimestamp(string sourceName)
        {
            try
            {
                var service = _registry.GetService(sourceName);
                if (service == null)
                {
                    return NotFound(new { error = $"Watchlist service '{sourceName}' not found" });
                }

                var timestamp = await service.GetLastUpdateTimestampAsync();
                return Ok(new
                {
                    SourceName = sourceName,
                    LastUpdateTimestamp = timestamp,
                    IsUpToDate = timestamp.HasValue && (DateTime.UtcNow - timestamp.Value).TotalDays < 7
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last update timestamp for source: {SourceName}", sourceName);
                return StatusCode(500, new { error = "Failed to get last update timestamp", details = ex.Message });
            }
        }

        /// <summary>
        /// Get last update timestamps for all watchlists
        /// </summary>
        [HttpGet("last-updates")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> GetAllLastUpdateTimestamps()
        {
            try
            {
                var services = _registry.GetActiveServices();
                var results = new Dictionary<string, object>();

                foreach (var service in services)
                {
                    try
                    {
                        var timestamp = await service.GetLastUpdateTimestampAsync();
                        results[service.SourceName] = new
                        {
                            LastUpdateTimestamp = timestamp,
                            IsUpToDate = timestamp.HasValue && (DateTime.UtcNow - timestamp.Value).TotalDays < 7,
                            DaysSinceUpdate = timestamp.HasValue ? (double?)(DateTime.UtcNow - timestamp.Value).TotalDays : null
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting last update timestamp for source: {SourceName}", service.SourceName);
                        results[service.SourceName] = new { Error = ex.Message };
                    }
                }

                return Ok(new
                {
                    Timestamp = DateTime.UtcNow,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all last update timestamps");
                return StatusCode(500, new { error = "Failed to get last update timestamps", details = ex.Message });
            }
        }
    }
}
