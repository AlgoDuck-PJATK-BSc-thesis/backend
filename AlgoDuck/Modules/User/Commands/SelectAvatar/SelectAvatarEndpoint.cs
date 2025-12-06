using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.SelectAvatar;

[ApiController]
[Route("api/user/avatar")]
public sealed class SelectAvatarEndpoint : ControllerBase
{
    private readonly ISelectAvatarHandler _handler;
    private readonly IValidator<SelectAvatarDto> _validator;

    public SelectAvatarEndpoint(
        ISelectAvatarHandler handler,
        IValidator<SelectAvatarDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> SelectAvatar(SelectAvatarDto dto, CancellationToken cancellationToken)
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