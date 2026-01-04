using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.User.Stats.GetUserStatistics;

[ApiController]
[Route("api/user/{userId:guid}/statistics")]
[Authorize]
public sealed class GetUserStatisticsByIdEndpoint : ControllerBase
{
    private readonly IGetUserStatisticsHandler _handler;

    public GetUserStatisticsByIdEndpoint(IGetUserStatisticsHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid userId, CancellationToken cancellationToken)
    {
        var statistics = await _handler.HandleAsync(userId, cancellationToken);

        var response = new StandardApiResponse<UserStatisticsDto>
        {
            Body = statistics
        };

        return Ok(response);
    }
}