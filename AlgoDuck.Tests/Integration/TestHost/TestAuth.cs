using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Tests.Integration.TestHost;

public static class TestAuth
{
    public const string Scheme = "Test";
    public const string HeaderName = "X-Test-Auth";
    public const string Admin = "admin";
    public const string User = "user";
}

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuth.HeaderName, out var values))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var mode = values.ToString().Trim().ToLowerInvariant();

        var userId = mode == TestAuth.Admin ? Guid.Parse("11111111-1111-1111-1111-111111111111") : Guid.Parse("22222222-2222-2222-2222-222222222222");
        var role = mode == TestAuth.Admin ? "Admin" : "User";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, TestAuth.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuth.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}