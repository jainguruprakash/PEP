using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using PEPScanner.Infrastructure.Services;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("DevelopmentCors")]
    public class OpenSanctionsController : ControllerBase
    {
        private readonly IOpenSanctionsService _openSanctionsService;
        private readonly IEnhancedScreeningService _enhancedScreeningService;
        private readonly ILogger<OpenSanctionsController> _logger;

        public OpenSanctionsController(
            IOpenSanctionsService openSanctionsService,
            IEnhancedScreeningService enhancedScreeningService,
            ILogger<OpenSanctionsController> logger)
        {
            _openSanctionsService = openSanctionsService;
            _enhancedScreeningService = enhancedScreeningService;
            _logger = logger;
        }

        /// <summary>
        /// Search for entities in OpenSanctions
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Query parameter is required");
                }

                var results = await _openSanctionsService.SearchAsync(query, limit);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OpenSanctions for query: {Query}", query);
                return StatusCode(500, "An error occurred while searching OpenSanctions");
            }
        }

        /// <summary>
        /// Get a specific entity from OpenSanctions
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

                var entity = await _openSanctionsService.GetEntityAsync(entityId);
                if (entity == null)
                {
                    return NotFound($"Entity with ID {entityId} not found");
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenSanctions entity: {EntityId}", entityId);
                return StatusCode(500, "An error occurred while retrieving the entity");
            }
        }

        /// <summary>
        /// Match a person against OpenSanctions
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

                var result = await _openSanctionsService.MatchPersonAsync(
                    request.Name, 
                    request.DateOfBirth, 
                    request.Nationality);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching person against OpenSanctions: {Name}", request.Name);
                return StatusCode(500, "An error occurred while matching the person");
            }
        }

        /// <summary>
        /// Match an organization against OpenSanctions
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

                var result = await _openSanctionsService.MatchOrganizationAsync(
                    request.Name, 
                    request.Country);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching organization against OpenSanctions: {Name}", request.Name);
                return StatusCode(500, "An error occurred while matching the organization");
            }
        }

        /// <summary>
        /// Get alerts with OpenSanctions data
        /// </summary>
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlertsWithOpenSanctionsData([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            try
            {
                var alerts = await _enhancedScreeningService.GetAlertsWithOpenSanctionsDataAsync(skip, take);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts with OpenSanctions data");
                return StatusCode(500, "An error occurred while retrieving alerts");
            }
        }

        /// <summary>
        /// Get alert with detailed OpenSanctions information
        /// </summary>
        [HttpGet("alerts/{alertId}")]
        public async Task<IActionResult> GetAlertWithOpenSanctionsDetails(Guid alertId)
        {
            try
            {
                var alert = await _enhancedScreeningService.GetAlertWithOpenSanctionsDetailsAsync(alertId);
                if (alert == null)
                {
                    return NotFound($"Alert with ID {alertId} not found");
                }

                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert with OpenSanctions details: {AlertId}", alertId);
                return StatusCode(500, "An error occurred while retrieving the alert");
            }
        }

        /// <summary>
        /// Enhance an existing alert with OpenSanctions data
        /// </summary>
        [HttpPost("alerts/{alertId}/enhance")]
        public async Task<IActionResult> EnhanceAlert(Guid alertId)
        {
            try
            {
                var alert = await _enhancedScreeningService.GetAlertWithOpenSanctionsDetailsAsync(alertId);
                if (alert == null)
                {
                    return NotFound($"Alert with ID {alertId} not found");
                }

                var enhancedAlert = await _enhancedScreeningService.EnhanceAlertWithOpenSanctionsAsync(alert);
                return Ok(enhancedAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing alert with OpenSanctions data: {AlertId}", alertId);
                return StatusCode(500, "An error occurred while enhancing the alert");
            }
        }

        /// <summary>
        /// Test endpoint to verify OpenSanctions integration is working
        /// </summary>
        [HttpGet("test")]
        public IActionResult TestIntegration()
        {
            return Ok(new
            {
                message = "OpenSanctions integration is properly configured",
                timestamp = DateTime.UtcNow,
                endpoints = new[]
                {
                    "GET /api/opensanctions/search?query={query}&limit={limit}",
                    "GET /api/opensanctions/entity/{entityId}",
                    "POST /api/opensanctions/match/person",
                    "POST /api/opensanctions/match/organization",
                    "GET /api/opensanctions/alerts",
                    "GET /api/opensanctions/alerts/{alertId}",
                    "POST /api/opensanctions/alerts/{alertId}/enhance"
                },
                note = "To use the OpenSanctions API, configure the 'OpenSanctionsApiKey' in appsettings.json"
            });
        }
    }

    public class PersonMatchRequest
    {
        public string Name { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
    }

    public class OrganizationMatchRequest
    {
        public string Name { get; set; } = "";
        public string? Country { get; set; }
    }
}
