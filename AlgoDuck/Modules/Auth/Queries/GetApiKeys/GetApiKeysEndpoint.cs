using System.Security.Claims;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Queries.GetApiKeys;

[ApiController]
[Route("api/auth/api-keys")]
public sealed class GetApiKeysEndpoint : ControllerBase
{
    private readonly IGetApiKeysHandler _handler;
    private readonly IValidator<Guid> _validator;

    public GetApiKeysEndpoint(
        IGetApiKeysHandler handler,
        IValidator<Guid> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ApiKeyDto>>> GetApiKeys(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _validator.ValidateAndThrowAsync(userId, cancellationToken);

        var apiKeys = await _handler.HandleAsync(userId, cancellationToken);
        return Ok(apiKeys);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(userIdClaim.Value, out var id) ? id : Guid.Empty;
    }
}