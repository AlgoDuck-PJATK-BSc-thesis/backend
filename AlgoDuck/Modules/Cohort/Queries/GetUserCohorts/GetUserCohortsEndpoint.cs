using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.GetUserCohorts;

[ApiController]
[Route("api/cohorts/me")]
[Authorize]
public sealed class GetUserCohortsEndpoint : ControllerBase
{
    private readonly IGetUserCohortsHandler _handler;

    public GetUserCohortsEndpoint(IGetUserCohortsHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<ActionResult<GetUserCohortsResultDto>> GetAsync(
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(userId, cancellationToken);
        return Ok(result);
    }
}