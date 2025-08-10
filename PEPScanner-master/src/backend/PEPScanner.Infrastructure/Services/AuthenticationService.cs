using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PEPScanner.Domain.Entities;
using PEPScanner.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PEPScanner.Infrastructure.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string username, string password);
        Task<AuthenticationResult> RegisterAsync(RegisterUserRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token);
        Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly PepScannerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthenticationService(
            PepScannerDbContext context,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _jwtSecret = _configuration["Jwt:Secret"] ?? "your-super-secret-jwt-key-that-should-be-at-least-32-characters-long";
            _jwtIssuer = _configuration["Jwt:Issuer"] ?? "PEPScanner";
            _jwtAudience = _configuration["Jwt:Audience"] ?? "PEPScanner-Users";
        }

        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                // Find user by username or email
                var orgUser = await _context.OrganizationUsers
                    .Include(ou => ou.Organization)
                    .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

                if (orgUser == null || !orgUser.IsActive)
                {
                    return AuthenticationResult.CreateFailed("Invalid credentials");
                }

                // Verify password
                if (!VerifyPassword(password, orgUser.PasswordHash))
                {
                    return AuthenticationResult.CreateFailed("Invalid credentials");
                }

                // Update last login
                orgUser.LastLoginAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(orgUser);
                var refreshToken = GenerateRefreshToken();

                // Store refresh token
                orgUser.RefreshToken = refreshToken;
                orgUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return AuthenticationResult.CreateSuccess(token, refreshToken, orgUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", username);
                return AuthenticationResult.CreateFailed("Login failed");
            }
        }

        public async Task<AuthenticationResult> RegisterAsync(RegisterUserRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

                if (existingUser != null)
                {
                    return AuthenticationResult.CreateFailed("User already exists");
                }

                // Validate role
                var validRoles = new[] { "Analyst", "ComplianceOfficer", "Manager", "Admin" };
                if (!validRoles.Contains(request.Role))
                {
                    return AuthenticationResult.CreateFailed("Invalid role specified");
                }

                // Create or find organization
                var organization = await GetOrCreateOrganizationAsync(request.OrganizationName);

                // Hash password
                var passwordHash = HashPassword(request.Password);

                // Create user
                var orgUser = new OrganizationUser
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organization.Id,
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = request.Role,
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _context.OrganizationUsers.Add(orgUser);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(orgUser);
                var refreshToken = GenerateRefreshToken();

                // Store refresh token
                orgUser.RefreshToken = refreshToken;
                orgUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return AuthenticationResult.CreateSuccess(token, refreshToken, orgUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
                return AuthenticationResult.CreateFailed("Registration failed");
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateLifetime = false, // Don't validate lifetime for refresh
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var orgUser = await _context.OrganizationUsers
                    .Include(ou => ou.Organization)
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (orgUser == null || orgUser.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return AuthenticationResult.CreateFailed("Invalid refresh token");
                }

                // Generate new tokens
                var newToken = GenerateJwtToken(orgUser);
                var newRefreshToken = GenerateRefreshToken();

                // Update refresh token
                orgUser.RefreshToken = newRefreshToken;
                orgUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return AuthenticationResult.CreateSuccess(newToken, newRefreshToken, orgUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return AuthenticationResult.CreateFailed("Token refresh failed");
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                var orgUser = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (orgUser != null)
                {
                    orgUser.RefreshToken = null;
                    orgUser.RefreshTokenExpiryTime = null;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var orgUser = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (orgUser == null || !VerifyPassword(currentPassword, orgUser.PasswordHash))
                {
                    return false;
                }

                orgUser.PasswordHash = HashPassword(newPassword);
                orgUser.PasswordChangedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return false;
            }
        }

        private string GenerateJwtToken(OrganizationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("OrganizationId", user.OrganizationId.ToString()),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private async Task<Organization> GetOrCreateOrganizationAsync(string organizationName)
        {
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Name == organizationName);

            if (organization == null)
            {
                organization = new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = organizationName,
                    Code = organizationName.Replace(" ", "").ToUpper(),
                    Type = "Bank",
                    CreatedAtUtc = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();
            }

            return organization;
        }
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? ErrorMessage { get; set; }
        public OrganizationUser? User { get; set; }

        public static AuthenticationResult CreateSuccess(string accessToken, string refreshToken, OrganizationUser user)
        {
            return new AuthenticationResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = user
            };
        }

        public static AuthenticationResult CreateFailed(string errorMessage)
        {
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class RegisterUserRequest
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Role { get; set; } = "";
        public string OrganizationName { get; set; } = "";
    }
}
