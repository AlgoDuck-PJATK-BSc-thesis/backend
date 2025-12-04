using System.Security.Cryptography;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.RefreshToken;

[ApiController]
[Route("api/auth")]
public sealed class RefreshTokenEndpoint : ControllerBase
{
    private readonly IRefreshTokenHandler _handler;
    private readonly JwtSettings _jwtSettings;
    private readonly IWebHostEnvironment _env;

    public RefreshTokenEndpoint(
        IRefreshTokenHandler handler,
        IOptions<JwtSettings> jwtOptions,
        IWebHostEnvironment env)
    {
        _handler = handler;
        _jwtSettings = jwtOptions.Value;
        _env = env;
    }

    [HttpPost("refresh-cqrs")]
    public async Task<ActionResult<RefreshResult>> RefreshAsync([FromBody] RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var ctx = HttpContext;

        string rawRefresh = dto.RefreshToken;

        if (string.IsNullOrWhiteSpace(rawRefresh))
        {
            var cookies = ctx.Request.Cookies;
            if (!cookies.TryGetValue(_jwtSettings.RefreshCookieName, out rawRefresh) ||
                string.IsNullOrWhiteSpace(rawRefresh))
                throw new UnauthorizedException("Missing refresh token");

            var header = Uri.UnescapeDataString(ctx.Request.Headers[_jwtSettings.CsrfHeaderName].ToString());
            if (!cookies.TryGetValue(_jwtSettings.CsrfCookieName, out var csrfCookie) || csrfCookie != header)
                throw new ForbiddenException("CSRF validation failed");
        }

        var command = new RefreshTokenDto
        {
            RefreshToken = rawRefresh
        };

        var result = await _handler.HandleAsync(command, cancellationToken);

        SetJwtCookie(Response, result.AccessToken, result.AccessTokenExpiresAt);
        SetRefreshCookie(Response, result.RefreshToken, result.RefreshTokenExpiresAt);
        SetCsrfCookie(Response);

        return Ok(result);
    }

    private void SetJwtCookie(HttpResponse response, string accessToken, DateTimeOffset expires)
    {
        var domain = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
        response.Cookies.Append(_jwtSettings.JwtCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = expires,
            Path = "/",
            Domain = domain
        });
    }

    private void SetRefreshCookie(HttpResponse response, string rawRefresh, DateTime refreshExpiresUtc)
    {
        var domain = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
        response.Cookies.Append(_jwtSettings.RefreshCookieName, rawRefresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = new DateTimeOffset(refreshExpiresUtc),
            Path = "/api/auth/refresh",
            Domain = domain
        });
    }

    private void SetCsrfCookie(HttpResponse response)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var domain = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
        response.Cookies.Append(_jwtSettings.CsrfCookieName, token, new CookieOptions
        {
            HttpOnly = false,
            Secure = !_env.IsDevelopment(),
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            Domain = domain
        });
        response.Headers[_jwtSettings.CsrfHeaderName] = token;
    }
}