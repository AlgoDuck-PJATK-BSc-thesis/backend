using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.StartEmailVerification;

[ApiController]
[Route("api/auth/email-verification/start")]
public sealed class StartEmailVerificationEndpoint : ControllerBase
{
    private readonly IStartEmailVerificationHandler _handler;

    public StartEmailVerificationEndpoint(IStartEmailVerificationHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Start([FromBody] StartEmailVerificationDto dto, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(dto, cancellationToken);

        return Ok(new { message = "If this email is registered, a verification link has been sent." });
    }
}