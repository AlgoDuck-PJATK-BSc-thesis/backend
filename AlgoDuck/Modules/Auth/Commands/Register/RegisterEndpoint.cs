using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.Register;

[ApiController]
[Route("api/auth/register")]
public sealed class RegisterEndpoint : ControllerBase
{
    private readonly IRegisterHandler _handler;

    public RegisterEndpoint(IRegisterHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        AuthUserDto user = await _handler.HandleAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(Register),
            new { userId = user.Id },
            new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.EmailConfirmed
            });
    }
}