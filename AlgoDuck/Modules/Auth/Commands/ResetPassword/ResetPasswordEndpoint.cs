using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.ResetPassword;

[ApiController]
[Route("api/auth/password-reset/reset")]
public sealed class ResetPasswordEndpoint : ControllerBase
{
    private readonly IResetPasswordHandler _handler;

    public ResetPasswordEndpoint(IResetPasswordHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Reset([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(dto, cancellationToken);

        return Ok(new { message = "Password has been reset successfully." });
    }
}