using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Queries.CheckUserPermissions;

[ApiController]
[Route("api/auth/permissions/check")]
public sealed class CheckUserPermissionsEndpoint : ControllerBase
{
    private readonly ICheckUserPermissionsHandler _handler;
    private readonly IValidator<PermissionsDto> _validator;

    public CheckUserPermissionsEndpoint(
        ICheckUserPermissionsHandler handler,
        IValidator<PermissionsDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<IDictionary<string, bool>>> CheckPermissions(PermissionsDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
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