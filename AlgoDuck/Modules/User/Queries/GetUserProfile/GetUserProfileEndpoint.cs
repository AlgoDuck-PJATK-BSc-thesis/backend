using System.Security.Claims;
using AlgoDuck.Modules.User.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

[ApiController]
[Route("api/user/profile")]
[Authorize]
public sealed class GetUserProfileEndpoint : ControllerBase
{
    private readonly IGetUserProfileHandler _handler;
    private readonly GetUserProfileValidator _validator;

    public GetUserProfileEndpoint(IGetUserProfileHandler handler, GetUserProfileValidator validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var validationResult = _validator.Validate(userId);
        if (!validationResult.IsValid)
        {
            return Unauthorized();
        }

        var profile = await _handler.HandleAsync(userId, cancellationToken);
        return Ok(profile);
    }
}