using AlgoDuck.Modules.User.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetLeaderboardGlobal;

[ApiController]
[Route("api/leaderboard")]
[Authorize]
public sealed class GetLeaderboardGlobalEndpoint : ControllerBase
{
    private readonly IGetLeaderboardGlobalHandler _handler;

    public GetLeaderboardGlobalEndpoint(IGetLeaderboardGlobalHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("global")]
    public async Task<ActionResult<UserLeaderboardPageDto>> Get([FromQuery] GetLeaderboardGlobalRequestDto requestDto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(requestDto, cancellationToken);
        return Ok(result);
    }
}