using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Utils;
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

        AuthCookieWriter.SetAuthCookies(Response, _jwtSettings, authResponse);

        return Ok(new
        {
            message = "External login successful.",
            userId = authResponse.UserId,
            sessionId = authResponse.SessionId,
            accessTokenExpiresAt = authResponse.AccessTokenExpiresAt,
            refreshTokenExpiresAt = authResponse.RefreshTokenExpiresAt
        });
    }
}