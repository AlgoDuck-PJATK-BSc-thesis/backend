using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.GenerateApiKey;

[ApiController]
[Route("api/auth/api-keys")]
public sealed class GenerateApiKeyEndpoint : ControllerBase
{
    private readonly IGenerateApiKeyHandler _handler;

    public GenerateApiKeyEndpoint(IGenerateApiKeyHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Generate([FromBody] GenerateApiKeyDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(User);
        if (userId is null)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await _handler.HandleAsync(userId.Value, dto, cancellationToken);

        return Ok(new
        {
            apiKey = result.ApiKey,
            rawKey = result.RawKey
        });
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