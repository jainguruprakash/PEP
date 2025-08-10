using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PEPScanner.Infrastructure.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Username and password are required" });
                }

                var result = await _authService.LoginAsync(request.Username, request.Password);

                if (!result.Success)
                {
                    return Unauthorized(new { error = result.ErrorMessage });
                }

                var response = new
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    User = new
                    {
                        Id = result.User!.Id,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        FirstName = result.User.FirstName,
                        LastName = result.User.LastName,
                        Role = result.User.Role,
                        OrganizationId = result.User.OrganizationId
                    }
                };

                return Ok(response);
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
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Username, email, and password are required" });
                }

                if (string.IsNullOrEmpty(request.Role))
                {
                    return BadRequest(new { error = "Role is required. Valid roles: Analyst, ComplianceOfficer, Manager, Admin" });
                }

                var registerUserRequest = new RegisterUserRequest
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = request.Password,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role,
                    OrganizationName = request.OrganizationName
                };

                var result = await _authService.RegisterAsync(registerUserRequest);

                if (!result.Success)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }

                var response = new
                {
                    Message = "User registered successfully",
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    User = new
                    {
                        Id = result.User!.Id,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        FirstName = result.User.FirstName,
                        LastName = result.User.LastName,
                        Role = result.User.Role,
                        OrganizationId = result.User.OrganizationId
                    }
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
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Analyst, ComplianceOfficer, Manager, Admin

        [Required]
        public string OrganizationName { get; set; } = string.Empty;

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
