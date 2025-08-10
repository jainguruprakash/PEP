using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using PEPScanner.Infrastructure.Services;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/organizations/{organizationId}/custom-lists")]
    [Authorize]
    [EnableCors("DevelopmentCors")]
    public class OrganizationCustomListController : ControllerBase
    {
        private readonly IOrganizationCustomListService _customListService;
        private readonly ILogger<OrganizationCustomListController> _logger;

        public OrganizationCustomListController(
            IOrganizationCustomListService customListService,
            ILogger<OrganizationCustomListController> logger)
        {
            _customListService = customListService;
            _logger = logger;
        }

        /// <summary>
        /// Get all custom lists for an organization
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomLists(Guid organizationId)
        {
            try
            {
                var customLists = await _customListService.GetCustomListsAsync(organizationId);
                return Ok(customLists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom lists for organization {OrganizationId}", organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a specific custom list
        /// </summary>
        [HttpGet("{listId}")]
        public async Task<IActionResult> GetCustomList(Guid organizationId, Guid listId)
        {
            try
            {
                var customList = await _customListService.GetCustomListAsync(organizationId, listId);
                if (customList == null)
                {
                    return NotFound($"Custom list {listId} not found for organization {organizationId}");
                }

                return Ok(customList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting custom list {ListId} for organization {OrganizationId}", listId, organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new custom list
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCustomList(Guid organizationId, [FromBody] CreateCustomListRequest request)
        {
            try
            {
                var customList = new OrganizationCustomList
                {
                    ListName = request.ListName,
                    ListType = request.ListType,
                    Description = request.Description,
                    RiskLevel = request.RiskLevel ?? "Medium",
                    MatchThreshold = request.MatchThreshold ?? 0.8,
                    AutoAlert = request.AutoAlert ?? true,
                    RequireReview = request.RequireReview ?? true,
                    ReviewRole = request.ReviewRole ?? "ComplianceOfficer",
                    UpdateFrequency = request.UpdateFrequency ?? "Manual",
                    CreatedBy = "API" // TODO: Get from authenticated user
                };

                var result = await _customListService.CreateCustomListAsync(organizationId, customList);
                return CreatedAtAction(nameof(GetCustomList), new { organizationId, listId = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating custom list for organization {OrganizationId}", organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Update a custom list
        /// </summary>
        [HttpPut("{listId}")]
        public async Task<IActionResult> UpdateCustomList(Guid organizationId, Guid listId, [FromBody] UpdateCustomListRequest request)
        {
            try
            {
                var customList = await _customListService.GetCustomListAsync(organizationId, listId);
                if (customList == null)
                {
                    return NotFound($"Custom list {listId} not found for organization {organizationId}");
                }

                customList.ListName = request.ListName ?? customList.ListName;
                customList.Description = request.Description ?? customList.Description;
                customList.RiskLevel = request.RiskLevel ?? customList.RiskLevel;
                customList.MatchThreshold = request.MatchThreshold ?? customList.MatchThreshold;
                customList.AutoAlert = request.AutoAlert ?? customList.AutoAlert;
                customList.RequireReview = request.RequireReview ?? customList.RequireReview;
                customList.ReviewRole = request.ReviewRole ?? customList.ReviewRole;
                customList.UpdateFrequency = request.UpdateFrequency ?? customList.UpdateFrequency;
                customList.IsActive = request.IsActive ?? customList.IsActive;
                customList.UpdatedBy = "API"; // TODO: Get from authenticated user

                var result = await _customListService.UpdateCustomListAsync(customList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating custom list {ListId} for organization {OrganizationId}", listId, organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a custom list
        /// </summary>
        [HttpDelete("{listId}")]
        public async Task<IActionResult> DeleteCustomList(Guid organizationId, Guid listId)
        {
            try
            {
                var success = await _customListService.DeleteCustomListAsync(organizationId, listId);
                if (!success)
                {
                    return NotFound($"Custom list {listId} not found for organization {organizationId}");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting custom list {ListId} for organization {OrganizationId}", listId, organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get entries in a custom list
        /// </summary>
        [HttpGet("{listId}/entries")]
        public async Task<IActionResult> GetCustomListEntries(Guid organizationId, Guid listId, [FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var entries = await _customListService.GetCustomListEntriesAsync(organizationId, listId, skip, take);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entries for custom list {ListId} in organization {OrganizationId}", listId, organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Add an entry to a custom list
        /// </summary>
        [HttpPost("{listId}/entries")]
        public async Task<IActionResult> AddCustomListEntry(Guid organizationId, Guid listId, [FromBody] CreateCustomListEntryRequest request)
        {
            try
            {
                var entry = new OrganizationCustomListEntry
                {
                    PrimaryName = request.PrimaryName,
                    FirstName = request.FirstName ?? "",
                    LastName = request.LastName ?? "",
                    DateOfBirth = request.DateOfBirth,
                    Nationality = request.Nationality ?? "",
                    Country = request.Country ?? "",
                    PositionOrRole = request.PositionOrRole ?? "",
                    EntryType = request.EntryType ?? "PEP",
                    RiskCategory = request.RiskCategory ?? "Medium",
                    CustomerType = request.CustomerType ?? "",
                    RelationshipType = request.RelationshipType ?? "",
                    Notes = request.Notes ?? "",
                    AddedBy = "API" // TODO: Get from authenticated user
                };

                var result = await _customListService.AddCustomListEntryAsync(organizationId, listId, entry);
                return CreatedAtAction(nameof(GetCustomListEntries), new { organizationId, listId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entry to custom list {ListId} for organization {OrganizationId}", listId, organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Import entries from file
        /// </summary>
        [HttpPost("{listId}/import")]
        public async Task<IActionResult> ImportFromFile(Guid organizationId, Guid listId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                var result = await _customListService.ImportFromFileAsync(organizationId, listId, file, "API");
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing file to custom list {ListId} for organization {OrganizationId}", listId, organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Search across all custom lists for an organization
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomLists(Guid organizationId, [FromQuery] string searchTerm, [FromQuery] string? listType = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("Search term is required");
                }

                var results = await _customListService.SearchCustomListsAsync(organizationId, searchTerm, listType);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching custom lists for organization {OrganizationId}", organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Match a person against all custom lists
        /// </summary>
        [HttpPost("match")]
        public async Task<IActionResult> MatchAgainstCustomLists(Guid organizationId, [FromBody] MatchPersonRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("Name is required");
                }

                var matches = await _customListService.MatchAgainstCustomListsAsync(
                    organizationId, 
                    request.Name, 
                    request.DateOfBirth, 
                    request.Nationality);

                return Ok(new
                {
                    query = request.Name,
                    totalMatches = matches.Count,
                    matches = matches,
                    searchedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching against custom lists for organization {OrganizationId}", organizationId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    // Request DTOs
    public class CreateCustomListRequest
    {
        public string ListName { get; set; } = "";
        public string ListType { get; set; } = "";
        public string? Description { get; set; }
        public string? RiskLevel { get; set; }
        public double? MatchThreshold { get; set; }
        public bool? AutoAlert { get; set; }
        public bool? RequireReview { get; set; }
        public string? ReviewRole { get; set; }
        public string? UpdateFrequency { get; set; }
    }

    public class UpdateCustomListRequest
    {
        public string? ListName { get; set; }
        public string? Description { get; set; }
        public string? RiskLevel { get; set; }
        public double? MatchThreshold { get; set; }
        public bool? AutoAlert { get; set; }
        public bool? RequireReview { get; set; }
        public string? ReviewRole { get; set; }
        public string? UpdateFrequency { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateCustomListEntryRequest
    {
        public string PrimaryName { get; set; } = "";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Country { get; set; }
        public string? PositionOrRole { get; set; }
        public string? EntryType { get; set; }
        public string? RiskCategory { get; set; }
        public string? CustomerType { get; set; }
        public string? RelationshipType { get; set; }
        public string? Notes { get; set; }
    }

    public class MatchPersonRequest
    {
        public string Name { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
    }
}
