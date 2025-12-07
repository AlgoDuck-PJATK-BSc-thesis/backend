using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.ExternalLogin;

[ApiController]
[Route("api/auth/external-login")]
public sealed class ExternalLoginEndpoint : ControllerBase
{
    private readonly IExternalLoginHandler _handler;
    private readonly JwtSettings _jwtSettings;

    public ExternalLoginEndpoint(
        IExternalLoginHandler handler,
        IOptions<JwtSettings> jwtOptions)
    {
        _handler = handler;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto dto, CancellationToken cancellationToken)
    {
        AuthResponse authResponse = await _handler.HandleAsync(dto, cancellationToken);

        SetAuthCookies(authResponse);

        return Ok(new
        {
            message = "External login successful.",
            userId = authResponse.UserId,
            sessionId = authResponse.SessionId,
            accessTokenExpiresAt = authResponse.AccessTokenExpiresAt,
            refreshTokenExpiresAt = authResponse.RefreshTokenExpiresAt
        });
    }

    private void SetAuthCookies(AuthResponse authResponse)
    {
        var cookieDomain = _jwtSettings.CookieDomain;
        var expires = authResponse.RefreshTokenExpiresAt.UtcDateTime;
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

        Response.Cookies.Append(_jwtSettings.AccessTokenCookieName, authResponse.AccessToken, jwtCookieOptions);
        Response.Cookies.Append(_jwtSettings.RefreshTokenCookieName, authResponse.RefreshToken, refreshCookieOptions);
        Response.Cookies.Append(_jwtSettings.CsrfCookieName, authResponse.CsrfToken, csrfCookieOptions);
    }
}