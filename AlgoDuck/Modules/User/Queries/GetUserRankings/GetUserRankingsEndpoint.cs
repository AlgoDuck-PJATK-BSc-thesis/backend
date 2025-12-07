using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserRankings;

[ApiController]
[Route("api/user/rankings")]
[Authorize]
public sealed class GetUserRankingsEndpoint : ControllerBase
{
    private readonly IGetUserRankingsHandler _handler;

    public GetUserRankingsEndpoint(IGetUserRankingsHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetUserRankingsQuery query, CancellationToken cancellationToken)
    {
        var rankings = await _handler.HandleAsync(query, cancellationToken);
        return Ok(rankings);
    }
}