using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.EnableTwoFactor;

[ApiController]
[Route("api/auth/twofactor/enable")]
public sealed class EnableTwoFactorEndpoint : ControllerBase
{
    private readonly IEnableTwoFactorHandler _handler;

    public EnableTwoFactorEndpoint(IEnableTwoFactorHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Enable([FromBody] EnableTwoFactorDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(User);
        if (userId is null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        await _handler.HandleAsync(userId.Value, dto, cancellationToken);

        return Ok(new { message = "Two-factor authentication has been enabled." });
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