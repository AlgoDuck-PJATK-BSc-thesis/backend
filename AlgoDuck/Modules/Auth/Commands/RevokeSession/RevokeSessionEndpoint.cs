using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.RevokeSession;

[ApiController]
[Route("auth/sessions")]
[Authorize]
public sealed class RevokeSessionEndpoint : ControllerBase
{
    private readonly IRevokeSessionHandler _handler;

    public RevokeSessionEndpoint(IRevokeSessionHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeSessionDto dto, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        await _handler.HandleAsync(userId, dto, cancellationToken);

        return NoContent();
    }
}