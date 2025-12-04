using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.Login;

[ApiController]
[Route("api/auth")]
public sealed class LoginEndpoint : ControllerBase
{
    private readonly ILoginHandler _handler;

    public LoginEndpoint(ILoginHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("login-cqrs")]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }
}