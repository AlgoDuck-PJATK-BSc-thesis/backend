using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.UpdatePreferences;

[ApiController]
[Route("api/user/preferences")]
public sealed class UpdatePreferencesEndpoint : ControllerBase
{
    private readonly IUpdatePreferencesHandler _handler;
    private readonly IValidator<UpdatePreferencesDto> _validator;

    public UpdatePreferencesEndpoint(
        IUpdatePreferencesHandler handler,
        IValidator<UpdatePreferencesDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences(UpdatePreferencesDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        await _handler.HandleAsync(userId, dto, cancellationToken);

        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}