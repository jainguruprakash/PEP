using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(PepScannerDbContext context, ILogger<OrganizationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var organizations = await _context.Organizations
                    .Select(o => new
                    {
                        o.Id,
                        o.Name,
                        o.Code,
                        o.Type,
                        o.Industry,
                        o.Country,
                        o.ContactPerson,
                        o.ContactEmail,
                        o.ContactPhone,
                        o.IsActive,
                        o.CreatedAtUtc
                    })
                    .ToListAsync();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return NotFound(new { error = "Organization not found" });
                }

                return Ok(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request)
        {
            try
            {
                var organization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Code = request.Code ?? string.Empty,
                    Description = request.Description ?? string.Empty,
                    Type = request.Type ?? "Bank",
                    Industry = request.Industry ?? "Financial Services",
                    Country = request.Country ?? "India",
                    ContactPerson = request.ContactPerson ?? string.Empty,
                    ContactEmail = request.ContactEmail ?? string.Empty,
                    ContactPhone = request.ContactPhone ?? string.Empty,
                    CreatedAtUtc = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizationRequest request)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return NotFound(new { error = "Organization not found" });
                }

                organization.Name = request.Name;
                organization.Code = request.Code ?? organization.Code;
                organization.Description = request.Description ?? organization.Description;
                organization.Type = request.Type ?? organization.Type;
                organization.Industry = request.Industry ?? organization.Industry;
                organization.Country = request.Country ?? organization.Country;
                organization.ContactPerson = request.ContactPerson ?? organization.ContactPerson;
                organization.ContactEmail = request.ContactEmail ?? organization.ContactEmail;
                organization.ContactPhone = request.ContactPhone ?? organization.ContactPhone;
                organization.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
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
                await _context.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organization {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetStatistics(Guid id)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(id);
                if (organization == null)
                {
                    return NotFound(new { error = "Organization not found" });
                }

                var stats = new
                {
                    TotalCustomers = await _context.Customers.CountAsync(c => c.OrganizationId == id),
                    TotalAlerts = await _context.Alerts.CountAsync(a => a.Customer!.OrganizationId == id),
                    OpenAlerts = await _context.Alerts.CountAsync(a => a.Customer!.OrganizationId == id && a.Status == "Open"),
                    TotalScreeningJobs = await _context.ScreeningJobs.CountAsync(),
                    LastScreeningDate = await _context.ScreeningJobs
                        .Where(s => s.Status == "Completed")
                        .OrderByDescending(s => s.CompletedAtUtc)
                        .Select(s => s.CompletedAtUtc)
                        .FirstOrDefaultAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization statistics {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetUsers(Guid id)
        {
            try
            {
                var users = await _context.OrganizationUsers
                    .Where(u => u.OrganizationId == id)
                    .Select(u => new
                    {
                        u.Id,
                        UserId = u.Id.ToString(),
                        UserName = u.Username,
                        u.Email,
                        u.Role,
                        u.IsActive,
                        u.CreatedAtUtc
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization users {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/watchlists")]
        public async Task<IActionResult> GetWatchlists(Guid id)
        {
            try
            {
                var watchlists = await _context.OrganizationWatchlists
                    .Where(w => w.OrganizationId == id)
                    .Select(w => new
                    {
                        w.Id,
                        Name = w.WatchlistSource,
                        Source = w.WatchlistSource,
                        IsActive = w.IsEnabled,
                        LastUpdatedUtc = w.LastUpdateAtUtc
                    })
                    .ToListAsync();

                return Ok(watchlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization watchlists {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/configurations")]
        public async Task<IActionResult> GetConfigurations(Guid id)
        {
            try
            {
                var configurations = await _context.OrganizationConfigurations
                    .Where(c => c.OrganizationId == id)
                    .Select(c => new
                    {
                        c.Id,
                        c.Key,
                        c.Value,
                        c.Category,
                        IsActive = true // OrganizationConfiguration doesn't have IsActive property
                    })
                    .ToListAsync();

                return Ok(configurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization configurations {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            try
            {
                var organizations = await _context.Organizations
                    .Where(o => o.Name.Contains(q) || o.Code.Contains(q))
                    .Select(o => new
                    {
                        o.Id,
                        o.Name,
                        o.Code,
                        o.Type,
                        o.Country
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching organizations with query {Query}", q);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class CreateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Industry { get; set; }
        public string? Country { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class UpdateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Industry { get; set; }
        public string? Country { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}
