using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailRequest;

[ApiController]
[Route("api/auth/email/change")]
public sealed class ChangeEmailRequestEndpoint : ControllerBase
{
    private readonly IChangeEmailRequestHandler _handler;
    private readonly IValidator<ChangeEmailRequestDto> _validator;

    public ChangeEmailRequestEndpoint(
        IChangeEmailRequestHandler handler,
        IValidator<ChangeEmailRequestDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost("start")]
    [Authorize]
    public async Task<IActionResult> Start(ChangeEmailRequestDto dto, CancellationToken cancellationToken)
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