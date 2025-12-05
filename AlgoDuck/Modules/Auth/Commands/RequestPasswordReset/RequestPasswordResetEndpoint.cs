using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.RequestPasswordReset;

[ApiController]
[Route("api/auth/password-reset/request")]
public sealed class RequestPasswordResetEndpoint : ControllerBase
{
    private readonly IRequestPasswordResetHandler _handler;

    public RequestPasswordResetEndpoint(IRequestPasswordResetHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RequestReset([FromBody] RequestPasswordResetDto dto, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(dto, cancellationToken);

        return Ok(new { message = "If this email is registered, a password reset link has been sent." });
    }
}