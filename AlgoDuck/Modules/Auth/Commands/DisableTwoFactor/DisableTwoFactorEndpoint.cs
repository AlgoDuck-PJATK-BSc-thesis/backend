using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.DisableTwoFactor;

[ApiController]
[Route("api/auth/twofactor/disable")]
public sealed class DisableTwoFactorEndpoint : ControllerBase
{
    private readonly IDisableTwoFactorHandler _handler;

    public DisableTwoFactorEndpoint(IDisableTwoFactorHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Disable([FromBody] DisableTwoFactorDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(User);
        if (userId is null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        await _handler.HandleAsync(userId.Value, dto, cancellationToken);

        return Ok(new { message = "Two-factor authentication has been disabled." });
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