using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserLeaderboardPosition;

[ApiController]
[Route("user/leaderboard")]
[Authorize]
public sealed class GetUserLeaderboardPositionEndpoint : ControllerBase
{
    private readonly IGetUserLeaderboardPositionHandler _handler;

    public GetUserLeaderboardPositionEndpoint(IGetUserLeaderboardPositionHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserLeaderboardPositionDto>> Get(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(userId, cancellationToken);

        return Ok(result);
    }
}