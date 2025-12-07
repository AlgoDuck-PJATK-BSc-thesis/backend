using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.RevokeApiKey;

[ApiController]
[Route("api/auth/api-keys/{id:guid}")]
public sealed class RevokeApiKeyEndpoint : ControllerBase
{
    private readonly IRevokeApiKeyHandler _handler;

    public RevokeApiKeyEndpoint(IRevokeApiKeyHandler handler)
    {
        _handler = handler;
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Revoke(Guid id, [FromBody] RevokeApiKeyDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(User);
        if (userId is null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        await _handler.HandleAsync(userId.Value, id, dto, cancellationToken);

        return Ok(new { message = "API key has been revoked." });
    }

    private static Guid? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
        if (idClaim is null)
        {
            return null;
        }

        if (Guid.TryParse(idClaim.Value, out var id))
        {
            return id;
        }

        return null;
    }
}