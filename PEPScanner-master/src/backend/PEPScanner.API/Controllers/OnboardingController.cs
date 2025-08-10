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

                // Check if organization already exists
                var existingOrg = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.Name == request.Organization.Name || o.Email == request.Organization.Email);

                if (existingOrg != null)
                {
                    return BadRequest(new { error = "Organization with this name or email already exists" });
                }

                // Check if admin user email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.AdminUser.Email);

                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email already exists" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Create Organization
                    var organization = new Organization
                    {
                        Id = Guid.NewGuid(),
                        Name = request.Organization.Name,
                        Type = request.Organization.Type,
                        RbiLicenseNumber = request.Organization.RbiLicenseNumber,
                        SwiftCode = request.Organization.SwiftCode,
                        Address = request.Organization.Address,
                        City = request.Organization.City,
                        State = request.Organization.State,
                        PinCode = request.Organization.PinCode,
                        PhoneNumber = request.Organization.PhoneNumber,
                        Email = request.Organization.Email,
                        Website = request.Organization.Website,
                        IsActive = true,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.Organizations.Add(organization);
                    await _context.SaveChangesAsync();

                    // Create Organization Configuration
                    var config = new OrganizationConfiguration
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organization.Id,
                        TimeZone = request.Configuration.TimeZone ?? "Asia/Kolkata",
                        Currency = request.Configuration.Currency ?? "INR",
                        RiskThreshold = request.Configuration.RiskThreshold ?? 70,
                        AlertRetentionDays = request.Configuration.AlertRetentionDays ?? 365,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.OrganizationConfigurations.Add(config);

                    // Create Admin User
                    var hashedPassword = HashPassword(request.AdminUser.Password);
                    var adminUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = request.AdminUser.Username,
                        Email = request.AdminUser.Email,
                        PasswordHash = hashedPassword,
                        FirstName = request.AdminUser.FirstName,
                        LastName = request.AdminUser.LastName,
                        PhoneNumber = request.AdminUser.PhoneNumber,
                        EmployeeId = request.AdminUser.EmployeeId,
                        Department = request.AdminUser.Department,
                        Designation = request.AdminUser.Designation,
                        Role = "Admin",
                        IsActive = true,
                        IsEmailVerified = true, // Auto-verify for admin during onboarding
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };

                    _context.Users.Add(adminUser);

                    // Create Organization-User relationship
                    var orgUser = new OrganizationUser
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organization.Id,
                        UserId = adminUser.Id,
                        Role = "Admin",
                        IsActive = true,
                        JoinedAtUtc = DateTime.UtcNow
                    };

                    _context.OrganizationUsers.Add(orgUser);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Generate auth response for immediate login
                    var authResponse = new
                    {
                        accessToken = GenerateJwtToken(adminUser, organization),
                        refreshToken = GenerateRefreshToken(),
                        tokenType = "Bearer",
                        expiresIn = 3600,
                        user = new
                        {
                            id = adminUser.Id,
                            username = adminUser.Username,
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

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email already exists" });
                }

                // Generate invitation token
                var inviteToken = GenerateInviteToken();
                var inviteExpiry = DateTime.UtcNow.AddDays(7); // 7 days expiry

                // Store invitation (you might want to create an Invitations table)
                // For now, we'll return the invite details
                
                var inviteLink = $"https://yourapp.com/accept-invite?token={inviteToken}";

                // TODO: Send email invitation
                
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
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
            return Convert.ToBase64String(hashedBytes);
        }

        private string GenerateJwtToken(User user, Organization organization)
        {
            // Mock JWT token - implement proper JWT generation
            return $"mock_jwt_token_{user.Id}_{organization.Id}";
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
        public OrganizationData Organization { get; set; } = new();
        public AdminUserData AdminUser { get; set; } = new();
        public ConfigurationData Configuration { get; set; } = new();
    }

    public class OrganizationData
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? RbiLicenseNumber { get; set; }
        public string? SwiftCode { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
    }

    public class AdminUserData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ConfigurationData
    {
        public string? TimeZone { get; set; }
        public string? Currency { get; set; }
        public int? RiskThreshold { get; set; }
        public int? AlertRetentionDays { get; set; }
    }

    public class UserInviteRequest
    {
        public Guid OrganizationId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
    }
}