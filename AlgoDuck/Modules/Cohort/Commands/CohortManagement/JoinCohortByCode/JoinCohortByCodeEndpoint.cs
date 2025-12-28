using System.Security.Claims;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohortByCode;

[ApiController]
[Route("api/cohorts/join")]
[Authorize]
public sealed class JoinCohortByCodeEndpoint : ControllerBase
{
    private readonly IJoinCohortByCodeHandler _handler;

    public JoinCohortByCodeEndpoint(IJoinCohortByCodeHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<ActionResult<JoinCohortResultDto>> JoinAsync(
        [FromBody] JoinCohortByCodeDto dto,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}