using System.Security.Claims;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

[ApiController]
[Route("api/user/profile")]
[Authorize]
public sealed class GetUserProfileEndpoint : ControllerBase
{
    private readonly IGetUserProfileHandler _handler;

    public GetUserProfileEndpoint(IGetUserProfileHandler handler)
    {
        _handler = handler;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            var error = new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Unauthorized"
            };

            return Unauthorized(error);
        }

        var profile = await _handler.HandleAsync(userId, cancellationToken);

        var response = new StandardApiResponse<UserProfileDto>
        {
            Body = profile
        };

        return Ok(response);
    }
}