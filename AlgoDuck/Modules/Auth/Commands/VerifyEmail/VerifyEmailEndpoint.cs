using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.VerifyEmail;

[ApiController]
[Route("api/auth/email-verification/verify")]
public sealed class VerifyEmailEndpoint : ControllerBase
{
    private readonly IVerifyEmailHandler _handler;

    public VerifyEmailEndpoint(IVerifyEmailHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Verify([FromBody] VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(dto, cancellationToken);

        return Ok(new { message = "Email verified successfully." });
    }
}