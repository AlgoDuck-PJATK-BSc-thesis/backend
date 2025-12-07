using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.RefreshToken;

[ApiController]
[Route("api/auth/refresh")]
public sealed class RefreshTokenEndpoint : ControllerBase
{
    private readonly IRefreshTokenHandler _handler;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenEndpoint(IRefreshTokenHandler handler, IOptions<JwtSettings> jwtOptions)
    {
        _handler = handler;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[_jwtSettings.RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token cookie is missing." });
        }

        var dto = new RefreshTokenDto
        {
            RefreshToken = refreshToken
        };

        RefreshResult result = await _handler.HandleAsync(dto, cancellationToken);

        SetAuthCookies(result);

        return Ok(new
        {
            message = "Tokens refreshed successfully.",
            userId = result.UserId,
            sessionId = result.SessionId,
            accessTokenExpiresAt = result.AccessTokenExpiresAt,
            refreshTokenExpiresAt = result.RefreshTokenExpiresAt
        });
    }

    private void SetAuthCookies(RefreshResult result)
    {
        var cookieDomain = _jwtSettings.CookieDomain;
        var expires = result.RefreshTokenExpiresAt.UtcDateTime;
        var secure = !string.IsNullOrWhiteSpace(cookieDomain);

        var jwtCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = expires
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = expires
        };

        var csrfCookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = expires
        };

        Response.Cookies.Append(_jwtSettings.AccessTokenCookieName, result.AccessToken, jwtCookieOptions);
        Response.Cookies.Append(_jwtSettings.RefreshTokenCookieName, result.RefreshToken, refreshCookieOptions);
        Response.Cookies.Append(_jwtSettings.CsrfCookieName, result.CsrfToken, csrfCookieOptions);
    }
}