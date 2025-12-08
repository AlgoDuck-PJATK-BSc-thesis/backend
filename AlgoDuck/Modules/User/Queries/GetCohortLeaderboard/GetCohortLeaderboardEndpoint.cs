using AlgoDuck.Modules.User.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetCohortLeaderboard;

[ApiController]
[Route("api/leaderboard")]
[Authorize]
public sealed class GetCohortLeaderboardEndpoint : ControllerBase
{
    private readonly IGetCohortLeaderboardHandler _handler;

    public GetCohortLeaderboardEndpoint(IGetCohortLeaderboardHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("cohort")]
    public async Task<ActionResult<UserLeaderboardPageDto>> Get([FromQuery] GetCohortLeaderboardRequestDto requestDto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(requestDto, cancellationToken);
        return Ok(result);
    }
}