using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PEPScanner.API.Dev;

public class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create a default user for development testing
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dev-user-123"),
            new Claim(ClaimTypes.Name, "Development User"),
            new Claim(ClaimTypes.Email, "dev@pepscanner.com"),
            new Claim(ClaimTypes.Role, "ComplianceOfficer"),
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim("OrganizationId", "dev-org-123")
        };

        var identity = new ClaimsIdentity(claims, "Dev");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Dev");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
