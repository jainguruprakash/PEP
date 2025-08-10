using Microsoft.AspNetCore.Mvc;
using PEPScanner.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using static PEPScanner.API.Controllers.OpenSanctionsController;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("DevelopmentCors")]
    public class OpenSanctionsDataController : ControllerBase
    {
        private readonly IOpenSanctionsDataService _openSanctionsDataService;
        private readonly ILogger<OpenSanctionsDataController> _logger;

        public OpenSanctionsDataController(
            IOpenSanctionsDataService openSanctionsDataService,
            ILogger<OpenSanctionsDataController> logger)
        {
            _openSanctionsDataService = openSanctionsDataService;
            _logger = logger;
        }

        /// <summary>
        /// Get OpenSanctions data status and statistics
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var lastUpdate = await _openSanctionsDataService.GetLastUpdateTimeAsync();
                var totalEntities = await _openSanctionsDataService.GetTotalEntitiesCountAsync();

                return Ok(new
                {
                    lastUpdate = lastUpdate,
                    totalEntities = totalEntities,
                    isDataAvailable = totalEntities > 0,
                    dataAge = lastUpdate.HasValue ? DateTime.UtcNow - lastUpdate.Value : (TimeSpan?)null,
                    recommendUpdate = !lastUpdate.HasValue || DateTime.UtcNow - lastUpdate.Value > TimeSpan.FromDays(1)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenSanctions data status");
                return StatusCode(500, "An error occurred while retrieving data status");
            }
        }

        /// <summary>
        /// Download and update OpenSanctions data
        /// </summary>
        [HttpPost("download")]
        public async Task<IActionResult> DownloadData()
        {
            try
            {
                _logger.LogInformation("Starting OpenSanctions data download via API request");
                
                var success = await _openSanctionsDataService.DownloadAndUpdateDataAsync();
                
                if (success)
                {
                    var totalEntities = await _openSanctionsDataService.GetTotalEntitiesCountAsync();
                    return Ok(new
                    {
                        success = true,
                        message = "OpenSanctions data downloaded and updated successfully",
                        totalEntities = totalEntities,
                        updatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Failed to download and update OpenSanctions data"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading OpenSanctions data");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while downloading data",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Search OpenSanctions entities
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchEntities([FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Query parameter is required");
                }

                var entities = await _openSanctionsDataService.SearchEntitiesAsync(query, limit);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OpenSanctions entities for query: {Query}", query);
                return StatusCode(500, "An error occurred while searching entities");
            }
        }

        /// <summary>
        /// Get a specific OpenSanctions entity by ID
        /// </summary>
        [HttpGet("entity/{entityId}")]
        public async Task<IActionResult> GetEntity(string entityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    return BadRequest("Entity ID is required");
                }

                var entity = await _openSanctionsDataService.GetEntityByIdAsync(entityId);
                if (entity == null)
                {
                    return NotFound($"Entity with ID {entityId} not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenSanctions entity by ID: {EntityId}", entityId);
                return StatusCode(500, "An error occurred while retrieving the entity");
            }
        }

        /// <summary>
        /// Match a person against local OpenSanctions data
        /// </summary>
        [HttpPost("match/person")]
        public async Task<IActionResult> MatchPerson([FromBody] PersonMatchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("Name is required");
                }

                var matches = await _openSanctionsDataService.MatchPersonAsync(
                    request.Name, 
                    request.DateOfBirth, 
                    request.Nationality);

                return Ok(new
                {
                    query = request.Name,
                    totalResults = matches.Count,
                    matches = matches,
                    searchedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching person against OpenSanctions: {Name}", request.Name);
                return StatusCode(500, "An error occurred while matching the person");
            }
        }

        /// <summary>
        /// Match an organization against local OpenSanctions data
        /// </summary>
        [HttpPost("match/organization")]
        public async Task<IActionResult> MatchOrganization([FromBody] OrganizationMatchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("Name is required");
                }

                var matches = await _openSanctionsDataService.MatchOrganizationAsync(
                    request.Name, 
                    request.Country);

                return Ok(new
                {
                    query = request.Name,
                    totalResults = matches.Count,
                    matches = matches,
                    searchedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching organization against OpenSanctions: {Name}", request.Name);
                return StatusCode(500, "An error occurred while matching the organization");
            }
        }

        /// <summary>
        /// Test endpoint to verify OpenSanctions local data integration
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestIntegration()
        {
            try
            {
                var lastUpdate = await _openSanctionsDataService.GetLastUpdateTimeAsync();
                var totalEntities = await _openSanctionsDataService.GetTotalEntitiesCountAsync();

                return Ok(new
                {
                    message = "OpenSanctions local data integration is working",
                    timestamp = DateTime.UtcNow,
                    dataStatus = new
                    {
                        lastUpdate = lastUpdate,
                        totalEntities = totalEntities,
                        isDataAvailable = totalEntities > 0
                    },
                    endpoints = new[]
                    {
                        "GET /api/opensanctionsdata/status",
                        "POST /api/opensanctionsdata/download",
                        "GET /api/opensanctionsdata/search?query={query}&limit={limit}",
                        "GET /api/opensanctionsdata/entity/{entityId}",
                        "POST /api/opensanctionsdata/match/person",
                        "POST /api/opensanctionsdata/match/organization"
                    },
                    note = "This uses locally downloaded OpenSanctions data - no API key required"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test integration endpoint");
                return StatusCode(500, "An error occurred during integration test");
            }
        }
    }


}
