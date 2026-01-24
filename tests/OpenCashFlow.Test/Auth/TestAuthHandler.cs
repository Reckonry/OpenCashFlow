using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    [Obsolete]
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock? clock = null)
        : base(options, logger, encoder, clock ?? new SystemClock()) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Default values
        var userId = "default-user";
        var companyId = "00000000-0000-0000-0000-000000000001";
        var role = "Admin";

        // Legge header se presente
        if (Request.Headers.TryGetValue("X-Test-UserId", out var headerUserId))
            userId = headerUserId!;
        
        if (Request.Headers.TryGetValue("X-Test-CompanyId", out var headerCompanyId))
            companyId = headerCompanyId!;

        if (Request.Headers.TryGetValue("X-Test-Role", out var headerRole))
            role = headerRole!;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, role),
            new Claim("TenantID", companyId),
            new Claim("UserID", userId),
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

}
