using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.RevokeOtherSessions;

[ApiController]
[Route("auth/sessions")]
[Authorize]
public sealed class RevokeOtherSessionsEndpoint : ControllerBase
{
    private readonly IRevokeOtherSessionsHandler _handler;

    public RevokeOtherSessionsEndpoint(IRevokeOtherSessionsHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("revoke-others")]
    public async Task<IActionResult> RevokeOthers([FromBody] RevokeOtherSessionsDto dto, CancellationToken cancellationToken)
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