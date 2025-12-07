using System.Security.Claims;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserConfig;

[ApiController]
[Route("api/user/config")]
[Authorize]
public sealed class GetUserConfigEndpoint : ControllerBase
{
    private readonly IGetUserConfigHandler _handler;
    private readonly GetUserConfigValidator _validator;

    public GetUserConfigEndpoint(
        IGetUserConfigHandler handler,
        GetUserConfigValidator validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Unauthorized"
            });
        }

        var query = new GetUserConfigRequestDto { UserId = userId };

        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new StandardApiResponse
            {
                Status = Status.Error,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            });
        }

        try
        {
            var config = await _handler.HandleAsync(query, cancellationToken);

            return Ok(new StandardApiResponse<UserConfigDto>
            {
                Body = config
            });
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = ex.Message
            });
        }
    }
}