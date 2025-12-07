using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;

[ApiController]
[Route("api/auth/twofactor/verify-login")]
public sealed class VerifyTwoFactorLoginEndpoint : ControllerBase
{
    private readonly IVerifyTwoFactorLoginHandler _handler;
    private readonly JwtSettings _jwtSettings;

    public VerifyTwoFactorLoginEndpoint(
        IVerifyTwoFactorLoginHandler handler,
        IOptions<JwtSettings> jwtOptions)
    {
        _handler = handler;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyLogin([FromBody] VerifyTwoFactorLoginDto dto, CancellationToken cancellationToken)
    {
        AuthResponse authResponse = await _handler.HandleAsync(dto, cancellationToken);

        SetAuthCookies(authResponse);

        return Ok(new
        {
            message = "Two-factor login successful.",
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