using System.Security.Claims;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Queries.GetCurrentUser;

[ApiController]
[Route("api/auth/me")]
public sealed class GetCurrentUserEndpoint : ControllerBase
{
    private readonly IGetCurrentUserHandler _handler;
    private readonly IValidator<Guid> _validator;

    public GetCurrentUserEndpoint(
        IGetCurrentUserHandler handler,
        IValidator<Guid> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _validator.ValidateAndThrowAsync(userId, cancellationToken);

        var user = await _handler.HandleAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
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