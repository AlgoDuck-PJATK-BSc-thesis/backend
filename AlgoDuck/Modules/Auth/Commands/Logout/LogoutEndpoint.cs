using System.Security.Claims;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.Logout;

[ApiController]
[Route("api/auth/logout")]
public sealed class LogoutEndpoint : ControllerBase
{
    private readonly ILogoutHandler _handler;
    private readonly JwtSettings _jwtSettings;

    public LogoutEndpoint(ILogoutHandler handler, IOptions<JwtSettings> jwtOptions)
    {
        _handler = handler;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(User);
        var sessionId = GetSessionIdFromClaims(User);

        await _handler.HandleAsync(dto, userId, sessionId, cancellationToken);

        ClearAuthCookies();

        return Ok(new { message = "Logged out successfully." });
    }

    private static Guid? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
        if (idClaim is null)
        {
            return null;
        }

        if (Guid.TryParse(idClaim.Value, out var id))
        {
            return id;
        }

        return null;
    }

    private static Guid? GetSessionIdFromClaims(ClaimsPrincipal user)
    {
        var sessionClaim = user.FindFirst("sid");
        if (sessionClaim is null)
        {
            return null;
        }

        if (Guid.TryParse(sessionClaim.Value, out var id))
        {
            return id;
        }

        return null;
    }

    private void ClearAuthCookies()
    {
        var cookieDomain = _jwtSettings.CookieDomain;
        var secure = !string.IsNullOrWhiteSpace(cookieDomain);

        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = DateTimeOffset.UtcNow.AddDays(-7)
        };

        var expiredCsrfOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = DateTimeOffset.UtcNow.AddDays(-7)
        };

        Response.Cookies.Append(_jwtSettings.AccessTokenCookieName, string.Empty, expiredOptions);
        Response.Cookies.Append(_jwtSettings.RefreshTokenCookieName, string.Empty, expiredOptions);
        Response.Cookies.Append(_jwtSettings.CsrfCookieName, string.Empty, expiredCsrfOptions);
    }
}