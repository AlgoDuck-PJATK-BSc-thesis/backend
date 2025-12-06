using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.DeleteAccount;

[ApiController]
[Route("api/user/account")]
public sealed class DeleteAccountEndpoint : ControllerBase
{
    private readonly IDeleteAccountHandler _handler;
    private readonly IValidator<DeleteAccountDto> _validator;

    public DeleteAccountEndpoint(
        IDeleteAccountHandler handler,
        IValidator<DeleteAccountDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto, CancellationToken cancellationToken)
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