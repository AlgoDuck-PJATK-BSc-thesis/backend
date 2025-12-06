using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailConfirm;

[ApiController]
[Route("api/auth/email/change")]
public sealed class ChangeEmailConfirmEndpoint : ControllerBase
{
    private readonly IChangeEmailConfirmHandler _handler;
    private readonly IValidator<ChangeEmailConfirmDto> _validator;

    public ChangeEmailConfirmEndpoint(
        IChangeEmailConfirmHandler handler,
        IValidator<ChangeEmailConfirmDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm(ChangeEmailConfirmDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        await _handler.HandleAsync(dto, cancellationToken);

        return NoContent();
    }
}