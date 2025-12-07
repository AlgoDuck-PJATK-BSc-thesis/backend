using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.SetEditorTheme;

[ApiController]
[Route("api/user/editor/theme")]
public sealed class SetEditorThemeEndpoint : ControllerBase
{
    private readonly ISetEditorThemeHandler _handler;
    private readonly IValidator<SetEditorThemeDto> _validator;

    public SetEditorThemeEndpoint(
        ISetEditorThemeHandler handler,
        IValidator<SetEditorThemeDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> SetTheme(SetEditorThemeDto dto, CancellationToken cancellationToken)
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