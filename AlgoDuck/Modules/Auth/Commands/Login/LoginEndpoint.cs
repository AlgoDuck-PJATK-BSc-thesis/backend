using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.Login;

[ApiController]
[Route("api/auth/login")]
public sealed class LoginEndpoint : ControllerBase
{
    private readonly ILoginHandler _handler;
    private readonly JwtSettings _jwtSettings;

    public LoginEndpoint(ILoginHandler handler, IOptions<JwtSettings> jwtOptions)
    {
        _handler = handler;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(dto, cancellationToken);

        if (!result.TwoFactorRequired)
        {
            if (result.Auth is null)
            {
                return StatusCode(500, new { message = "Login result is invalid." });
            }

            SetAuthCookies(result.Auth);

            return Ok(new
            {
                message = "Logged in successfully.",
                twoFactorRequired = false,
                userId = result.Auth.UserId,
                sessionId = result.Auth.SessionId,
                accessTokenExpiresAt = result.Auth.AccessTokenExpiresAt,
                refreshTokenExpiresAt = result.Auth.RefreshTokenExpiresAt
            });
        }

        return Ok(new
        {
            message = "Two-factor authentication required.",
            twoFactorRequired = true,
            challengeId = result.ChallengeId,
            expiresAt = result.ExpiresAt
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