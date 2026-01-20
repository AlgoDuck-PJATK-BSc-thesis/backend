using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.Login.ExternalLogin;

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
        var result = await _handler.HandleAsync(dto, cancellationToken);

        if (result.TwoFactorRequired)
        {
            return Ok(new
            {
                message = result.Message,
                twoFactorRequired = true,
                challengeId = result.ChallengeId,
                expiresAt = result.ExpiresAt
            });
        }

        var authResponse = result.Auth;
        if (authResponse is null)
        {
            return StatusCode(500, "Missing auth response.");
        }

        AuthCookieWriter.SetAuthCookies(Response, _jwtSettings, authResponse);

        return Ok(new
        {
            message = result.Message,
            twoFactorRequired = false,
            userId = authResponse.UserId,
            sessionId = authResponse.SessionId,
            accessTokenExpiresAt = authResponse.AccessTokenExpiresAt,
            refreshTokenExpiresAt = authResponse.RefreshTokenExpiresAt
        });
    }
}