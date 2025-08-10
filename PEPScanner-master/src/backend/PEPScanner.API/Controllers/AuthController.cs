using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEPScanner.Infrastructure.Data;
using PEPScanner.Domain.Entities;
using System.Security.Claims;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PepScannerDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(PepScannerDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // For development, we'll use simple username/password validation
                // In production, this should use proper password hashing and JWT tokens
                
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                // Development authentication - replace with real authentication
                if (request.Username == "admin" && request.Password == "admin123")
                {
                    var response = new
                    {
                        AccessToken = "dev-token",
                        TokenType = "Bearer",
                        ExpiresIn = 3600,
                        User = new
                        {
                            Id = "dev-user-123",
                            Username = "admin",
                            Email = "admin@pepscanner.com",
                            Role = "Administrator",
                            OrganizationId = "dev-org-123"
                        }
                    };

                    return Ok(response);
                }

                return Unauthorized(new { error = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { error = "Email is required" });
                }

                // For development, we'll create a simple user record
                // In production, this should hash passwords and validate email uniqueness
                
                var userId = Guid.NewGuid();
                var organizationId = Guid.NewGuid();

                // Create organization if provided
                if (!string.IsNullOrEmpty(request.OrganizationName))
                {
                    var organization = new Organization
                    {
                        Id = organizationId,
                        Name = request.OrganizationName,
                        Code = request.OrganizationCode ?? string.Empty,
                        Type = "Bank",
                        Industry = "Financial Services",
                        Country = request.Country ?? "India",
                        ContactPerson = request.Username,
                        ContactEmail = request.Email,
                        CreatedAtUtc = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Organizations.Add(organization);
                }

                // Create organization user
                var orgUser = new OrganizationUser
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Username = request.Username,
                    Email = request.Email,
                    Role = "Administrator",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.OrganizationUsers.Add(orgUser);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Message = "User registered successfully",
                    UserId = userId,
                    OrganizationId = organizationId,
                    AccessToken = "bank-onboarding-dev-token"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // In a real implementation, this would invalidate the JWT token
                // For development, we just return success
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Get user info from claims (set by DevAuthenticationHandler)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var userInfo = new
                {
                    Id = userId,
                    Username = userName,
                    Email = email,
                    Roles = roles,
                    OrganizationId = organizationId,
                    IsAuthenticated = true
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                // In a real implementation, this would validate and refresh JWT tokens
                // For development, we just return a new dev token
                var response = new
                {
                    AccessToken = "dev-token-refreshed",
                    TokenType = "Bearer",
                    ExpiresIn = 3600
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { error = "Current password and new password are required" });
                }

                // In a real implementation, this would validate current password and update with hashed new password
                // For development, we just return success
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? OrganizationName { get; set; }
        public string? OrganizationCode { get; set; }
        public string? Country { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
