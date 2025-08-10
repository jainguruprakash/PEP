using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(PepScannerDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? role = null, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var usersQuery = _context.Users.AsQueryable();

                if (activeOnly)
                    usersQuery = usersQuery.Where(u => u.IsActive);

                if (!string.IsNullOrEmpty(role))
                    usersQuery = usersQuery.Where(u => u.Role == role);

                var users = await usersQuery
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.FullName,
                        u.Role,
                        u.Department,
                        u.JobTitle,
                        u.IsActive,
                        u.CanReviewAlerts,
                        u.CanApproveAlerts,
                        u.CanAssignAlerts,
                        u.LastLoginUtc
                    })
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                var result = new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.FullName,
                    user.Role,
                    user.Department,
                    user.JobTitle,
                    user.Phone,
                    user.IsActive,
                    user.CanCreateAlerts,
                    user.CanReviewAlerts,
                    user.CanApproveAlerts,
                    user.CanEscalateAlerts,
                    user.CanCloseAlerts,
                    user.CanViewAllAlerts,
                    user.CanAssignAlerts,
                    user.CanGenerateReports,
                    user.LastLoginUtc,
                    user.CreatedAtUtc
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                var result = new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.FullName,
                    user.Role,
                    user.Department,
                    user.IsActive,
                    user.CanReviewAlerts,
                    user.CanApproveAlerts,
                    user.CanAssignAlerts
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email {Email}", email);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("compliance-officers")]
        public async Task<IActionResult> GetComplianceOfficers()
        {
            try
            {
                var officers = await _context.Users
                    .Where(u => u.IsActive && (u.Role == "ComplianceOfficer" || u.Role == "Manager") && u.CanApproveAlerts)
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.FullName,
                        u.Role,
                        u.Department
                    })
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                return Ok(officers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance officers");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email already exists" });
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role,
                    Department = request.Department,
                    JobTitle = request.JobTitle,
                    Phone = request.Phone,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy ?? "System"
                };

                // Set permissions based on role
                SetPermissionsByRole(user, request.Role);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { user.Id, user.Email, user.FullName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private void SetPermissionsByRole(User user, string role)
        {
            switch (role)
            {
                case "Analyst":
                    user.CanCreateAlerts = true;
                    user.CanReviewAlerts = true;
                    user.CanViewAllAlerts = false;
                    break;
                case "ComplianceOfficer":
                    user.CanCreateAlerts = true;
                    user.CanReviewAlerts = true;
                    user.CanApproveAlerts = true;
                    user.CanAssignAlerts = true;
                    user.CanCloseAlerts = true;
                    user.CanViewAllAlerts = true;
                    break;
                case "Manager":
                    user.CanCreateAlerts = true;
                    user.CanReviewAlerts = true;
                    user.CanApproveAlerts = true;
                    user.CanEscalateAlerts = true;
                    user.CanAssignAlerts = true;
                    user.CanCloseAlerts = true;
                    user.CanViewAllAlerts = true;
                    user.CanGenerateReports = true;
                    break;
                case "Admin":
                    user.CanCreateAlerts = true;
                    user.CanReviewAlerts = true;
                    user.CanApproveAlerts = true;
                    user.CanEscalateAlerts = true;
                    user.CanAssignAlerts = true;
                    user.CanCloseAlerts = true;
                    user.CanViewAllAlerts = true;
                    user.CanGenerateReports = true;
                    break;
            }
        }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string? Phone { get; set; }
        public string? CreatedBy { get; set; }
    }
}
