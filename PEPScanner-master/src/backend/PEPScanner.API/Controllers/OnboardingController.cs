using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class OnboardingController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(PepScannerDbContext context, ILogger<OnboardingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("onboard-organization")]
        public async Task<IActionResult> OnboardOrganization([FromBody] OrganizationOnboardingRequest request)
        {
            try
            {
                _logger.LogInformation("Starting organization onboarding for: {OrganizationName}", request.Organization.Name);

                var existingOrg = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.Name == request.Organization.Name || o.ContactEmail == request.Organization.Email);

                if (existingOrg != null)
                {
                    return BadRequest(new { error = "Organization with this name or email already exists" });
                }

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.AdminUser.Email);

                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email already exists" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var organization = new Organization
                    {
                        Id = Guid.NewGuid(),
                        Name = request.Organization.Name,
                        Type = request.Organization.Type,
                        LicenseNumber = request.Organization.RbiLicenseNumber,
                        Address = request.Organization.Address,
                        City = request.Organization.City,
                        State = request.Organization.State,
                        PostalCode = request.Organization.PinCode,
                        ContactPhone = request.Organization.PhoneNumber,
                        ContactEmail = request.Organization.Email,
                        Website = request.Organization.Website,
                        IsActive = true,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.Organizations.Add(organization);
                    await _context.SaveChangesAsync();

                    var config = new OrganizationConfiguration
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organization.Id,
                        Category = "General",
                        Key = "TimeZone",
                        Value = request.Configuration.TimeZone ?? "Asia/Kolkata",
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    _context.OrganizationConfigurations.Add(config);

                    var hashedPassword = HashPassword(request.AdminUser.Password);
                    var adminUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = request.AdminUser.Email,
                        PasswordHash = hashedPassword,
                        FirstName = request.AdminUser.FirstName,
                        LastName = request.AdminUser.LastName,
                        Phone = request.AdminUser.PhoneNumber,
                        JobTitle = request.AdminUser.EmployeeId,
                        Department = request.AdminUser.Department,
                        Role = "Admin",
                        IsActive = true,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.Users.Add(adminUser);

                    var orgUser = new OrganizationUser
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organization.Id,
                        Username = adminUser.Email,
                        Email = adminUser.Email,
                        FirstName = adminUser.FirstName,
                        LastName = adminUser.LastName,
                        FullName = $"{adminUser.FirstName} {adminUser.LastName}",
                        Role = "Admin",
                        Department = adminUser.Department,
                        PasswordHash = hashedPassword,
                        IsActive = true,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    _context.OrganizationUsers.Add(orgUser);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var authResponse = new
                    {
                        accessToken = GenerateJwtToken(adminUser, organization),
                        refreshToken = GenerateRefreshToken(),
                        tokenType = "Bearer",
                        expiresIn = 3600,
                        user = new
                        {
                            id = adminUser.Id,
                            username = adminUser.Email,
                            email = adminUser.Email,
                            firstName = adminUser.FirstName,
                            lastName = adminUser.LastName,
                            role = adminUser.Role,
                            organizationId = organization.Id
                        }
                    };

                    _logger.LogInformation("Organization onboarding completed successfully for: {OrganizationName}", organization.Name);

                    return Ok(new
                    {
                        message = "Organization onboarding completed successfully",
                        organizationId = organization.Id,
                        adminUserId = adminUser.Id,
                        authResponse = authResponse
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during organization onboarding");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost("invite-user")]
        public async Task<IActionResult> InviteUser([FromBody] UserInviteRequest request)
        {
            try
            {
                _logger.LogInformation("Inviting user: {Email} to organization: {OrganizationId}", request.Email, request.OrganizationId);

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email already exists" });
                }

                var inviteToken = GenerateInviteToken();
                var inviteExpiry = DateTime.UtcNow.AddDays(7);
                var inviteLink = $"https://yourapp.com/accept-invite?token={inviteToken}";

                _logger.LogInformation("User invitation created for: {Email}", request.Email);

                return Ok(new
                {
                    message = "User invitation sent successfully",
                    inviteToken = inviteToken,
                    inviteLink = inviteLink,
                    expiresAt = inviteExpiry
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting user");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private string GenerateJwtToken(User user, Organization organization)
        {
            return "jwt-token-placeholder";
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateInviteToken()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class OrganizationOnboardingRequest
    {
        public OrganizationDto Organization { get; set; } = new();
        public AdminUserDto AdminUser { get; set; } = new();
        public ConfigurationDto Configuration { get; set; } = new();
    }

    public class OrganizationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RbiLicenseNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
    }

    public class AdminUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class ConfigurationDto
    {
        public string TimeZone { get; set; } = string.Empty;
    }

    public class UserInviteRequest
    {
        public string Email { get; set; } = string.Empty;
        public Guid OrganizationId { get; set; }
    }
}