using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.API.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(
            PepScannerDbContext context,
            ILogger<OrganizationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all organizations
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<List<Organization>>> GetOrganizations()
        {
            try
            {
                var organizations = await _context.Organizations
                    .Where(o => o.IsActive)
                    .OrderBy(o => o.Name)
                    .ToListAsync();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations");
                return StatusCode(500, new { error = "Failed to get organizations", details = ex.Message });
            }
        }

        /// <summary>
        /// Get organization by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<Organization>> GetOrganization(Guid id)
        {
            try
            {
                var organization = await _context.Organizations
                    .Include(o => o.Users)
                    .Include(o => o.Watchlists)
                    .Include(o => o.Configurations)
                    .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

                if (organization == null)
                {
                    return NotFound(new { error = "Organization not found" });
                }

                return Ok(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization {Id}", id);
                return StatusCode(500, new { error = "Failed to get organization", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new organization
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<Organization>> CreateOrganization([FromBody] Organization organization)
        {
            try
            {
                if (await _context.Organizations.AnyAsync(o => o.Code == organization.Code))
                {
                    return BadRequest(new { error = "Organization code already exists" });
                }

                organization.Id = Guid.NewGuid();
                organization.CreatedAtUtc = DateTime.UtcNow;
                organization.IsActive = true;

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created organization: {Name} ({Code})", organization.Name, organization.Code);

                return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                return StatusCode(500, new { error = "Failed to create organization", details = ex.Message });
            }
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult<Organization>> UpdateOrganization(Guid id, [FromBody] Organization organization)
        {
            try
            {
                var existingOrganization = await _context.Organizations.FindAsync(id);
                if (existingOrganization == null)
                {
                    return NotFound(new { error = "Organization not found" });
                }

                // Check if code is being changed and if it already exists
                if (organization.Code != existingOrganization.Code &&
                    await _context.Organizations.AnyAsync(o => o.Code == organization.Code))
                {
                    return BadRequest(new { error = "Organization code already exists" });
                }

                // Update properties
                existingOrganization.Name = organization.Name;
                existingOrganization.Code = organization.Code;
                existingOrganization.Description = organization.Description;
                existingOrganization.Type = organization.Type;
                existingOrganization.Industry = organization.Industry;
                existingOrganization.Country = organization.Country;
                existingOrganization.State = organization.State;
                existingOrganization.City = organization.City;
                existingOrganization.Address = organization.Address;
                existingOrganization.PostalCode = organization.PostalCode;
                existingOrganization.ContactPerson = organization.ContactPerson;
                existingOrganization.ContactEmail = organization.ContactEmail;
                existingOrganization.ContactPhone = organization.ContactPhone;
                existingOrganization.Website = organization.Website;
                existingOrganization.LicenseNumber = organization.LicenseNumber;
                existingOrganization.RegulatoryBody = organization.RegulatoryBody;
                existingOrganization.SubscriptionPlan = organization.SubscriptionPlan;
                existingOrganization.MaxUsers = organization.MaxUsers;
                existingOrganization.MaxCustomers = organization.MaxCustomers;
                existingOrganization.IsTrial = organization.IsTrial;
                existingOrganization.TimeZone = organization.TimeZone;
                existingOrganization.Currency = organization.Currency;
                existingOrganization.Language = organization.Language;
                existingOrganization.UpdatedAtUtc = DateTime.UtcNow;
                existingOrganization.UpdatedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated organization: {Name} ({Code})", organization.Name, organization.Code);

                return Ok(existingOrganization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization {Id}", id);
                return StatusCode(500, new { error = "Failed to update organization", details = ex.Message });
            }
        }

        /// <summary>
        /// Deactivate organization
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Manager")]
        public async Task<ActionResult> DeactivateOrganization(Guid id)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return NotFound(new { error = "Organization not found" });
                }

                organization.IsActive = false;
                organization.UpdatedAtUtc = DateTime.UtcNow;
                organization.UpdatedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deactivated organization: {Name} ({Code})", organization.Name, organization.Code);

                return Ok(new { message = "Organization deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating organization {Id}", id);
                return StatusCode(500, new { error = "Failed to deactivate organization", details = ex.Message });
            }
        }

        /// <summary>
        /// Get organization statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<object>> GetOrganizationStatistics(Guid id)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return NotFound(new { error = "Organization not found" });
                }

                var statistics = new
                {
                    OrganizationId = id,
                    OrganizationName = organization.Name,
                    TotalCustomers = await _context.Customers.CountAsync(c => c.OrganizationId == id && c.IsActive),
                    TotalUsers = await _context.OrganizationUsers.CountAsync(u => u.OrganizationId == id && u.IsActive),
                    PepCustomers = await _context.Customers.CountAsync(c => c.OrganizationId == id && c.IsPep && c.IsActive),
                    HighRiskCustomers = await _context.Customers.CountAsync(c => c.OrganizationId == id && c.RiskLevel == "High" && c.IsActive),
                    TotalAlerts = await _context.Alerts.CountAsync(a => a.Customer.OrganizationId == id),
                    PendingAlerts = await _context.Alerts.CountAsync(a => a.Customer.OrganizationId == id && a.Status == "Open"),
                    TotalWatchlists = await _context.OrganizationWatchlists.CountAsync(w => w.OrganizationId == id && w.IsEnabled),
                    SubscriptionPlan = organization.SubscriptionPlan,
                    MaxUsers = organization.MaxUsers,
                    MaxCustomers = organization.MaxCustomers,
                    IsTrial = organization.IsTrial,
                    SubscriptionEndDate = organization.SubscriptionEndDate
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization statistics {Id}", id);
                return StatusCode(500, new { error = "Failed to get organization statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Get organization users
        /// </summary>
        [HttpGet("{id}/users")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<OrganizationUser>>> GetOrganizationUsers(Guid id)
        {
            try
            {
                var users = await _context.OrganizationUsers
                    .Where(u => u.OrganizationId == id && u.IsActive)
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization users {Id}", id);
                return StatusCode(500, new { error = "Failed to get organization users", details = ex.Message });
            }
        }

        /// <summary>
        /// Get organization watchlists
        /// </summary>
        [HttpGet("{id}/watchlists")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<OrganizationWatchlist>>> GetOrganizationWatchlists(Guid id)
        {
            try
            {
                var watchlists = await _context.OrganizationWatchlists
                    .Where(w => w.OrganizationId == id)
                    .OrderBy(w => w.Priority)
                    .ToListAsync();

                return Ok(watchlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization watchlists {Id}", id);
                return StatusCode(500, new { error = "Failed to get organization watchlists", details = ex.Message });
            }
        }

        /// <summary>
        /// Get organization configurations
        /// </summary>
        [HttpGet("{id}/configurations")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<OrganizationConfiguration>>> GetOrganizationConfigurations(Guid id)
        {
            try
            {
                var configurations = await _context.OrganizationConfigurations
                    .Where(c => c.OrganizationId == id)
                    .OrderBy(c => c.Category)
                    .ThenBy(c => c.Key)
                    .ToListAsync();

                return Ok(configurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization configurations {Id}", id);
                return StatusCode(500, new { error = "Failed to get organization configurations", details = ex.Message });
            }
        }

        /// <summary>
        /// Search organizations
        /// </summary>
        [HttpGet("search")]
        [Authorize(Policy = "ComplianceOfficer")]
        public async Task<ActionResult<List<Organization>>> SearchOrganizations([FromQuery] string? name, [FromQuery] string? type, [FromQuery] string? country)
        {
            try
            {
                var query = _context.Organizations.Where(o => o.IsActive);

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(o => o.Name.Contains(name) || o.Code.Contains(name));
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(o => o.Type == type);
                }

                if (!string.IsNullOrEmpty(country))
                {
                    query = query.Where(o => o.Country == country);
                }

                var organizations = await query
                    .OrderBy(o => o.Name)
                    .ToListAsync();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching organizations");
                return StatusCode(500, new { error = "Failed to search organizations", details = ex.Message });
            }
        }
    }
}
