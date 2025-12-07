using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.ChangePassword;

[ApiController]
[Route("api/user/password/change")]
public sealed class ChangePasswordEndpoint : ControllerBase
{
    private readonly IChangePasswordHandler _handler;
    private readonly IValidator<ChangePasswordDto> _validator;

    public ChangePasswordEndpoint(
        IChangePasswordHandler handler,
        IValidator<ChangePasswordDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto, CancellationToken cancellationToken)
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(userIdClaim.Value, out var id) ? id : Guid.Empty;
    }
}