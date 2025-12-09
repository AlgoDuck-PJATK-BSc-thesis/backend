using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/join")]
[Authorize]
public sealed class JoinCohortEndpoint : ControllerBase
{
    private readonly IJoinCohortHandler _handler;

    public JoinCohortEndpoint(IJoinCohortHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<ActionResult<JoinCohortResultDto>> JoinAsync(
        Guid cohortId,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(userId, cohortId, cancellationToken);
        return Ok(result);
    }
}