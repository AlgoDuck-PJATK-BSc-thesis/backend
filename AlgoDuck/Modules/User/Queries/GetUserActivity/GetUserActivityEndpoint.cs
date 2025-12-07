using System.Security.Claims;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserActivity;

[ApiController]
[Route("api/user/activity")]
[Authorize]
public sealed class GetUserActivityEndpoint : ControllerBase
{
    private readonly IGetUserActivityHandler _handler;

    public GetUserActivityEndpoint(IGetUserActivityHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetUserActivityRequestDto requestDto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            var unauthorizedResponse = new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Unauthorized"
            };

            return Unauthorized(unauthorizedResponse);
        }

        var activity = await _handler.HandleAsync(userId, requestDto, cancellationToken);

        var response = new StandardApiResponse<IReadOnlyList<UserActivityDto>>
        {
            Body = activity
        };

        return Ok(response);
    }
}