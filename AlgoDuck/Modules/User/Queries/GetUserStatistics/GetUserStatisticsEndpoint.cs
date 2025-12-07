using System.Security.Claims;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserStatistics;

[ApiController]
[Route("api/user/statistics")]
[Authorize]
public sealed class GetUserStatisticsEndpoint : ControllerBase
{
    private readonly IGetUserStatisticsHandler _handler;

    public GetUserStatisticsEndpoint(IGetUserStatisticsHandler handler)
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

        var statistics = await _handler.HandleAsync(userId, cancellationToken);

        var response = new StandardApiResponse<UserStatisticsDto>
        {
            Body = statistics
        };

        return Ok(response);
    }
}