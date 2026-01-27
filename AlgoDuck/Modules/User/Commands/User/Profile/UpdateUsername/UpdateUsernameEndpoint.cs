using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Modules.User.Commands.User.Profile.UpdateUsername;

[ApiController]
[Route("api/user/username")]
public sealed class UpdateUsernameEndpoint : ControllerBase
{
    private readonly IUpdateUsernameHandler _handler;
    private readonly IValidator<UpdateUsernameDto> _validator;

    public UpdateUsernameEndpoint(
        IUpdateUsernameHandler handler,
        IValidator<UpdateUsernameDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        try
        {
            await _handler.HandleAsync(userId, dto, cancellationToken);
            return Ok(new { message = "Username updated successfully" });
        }
        catch (Shared.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
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