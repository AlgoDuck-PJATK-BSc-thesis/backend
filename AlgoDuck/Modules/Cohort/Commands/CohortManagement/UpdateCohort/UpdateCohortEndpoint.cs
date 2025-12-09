using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.UpdateCohort;

[ApiController]
[Route("api/cohorts/{cohortId:guid}")]
[Authorize]
public sealed class UpdateCohortEndpoint : ControllerBase
{
    private readonly IUpdateCohortHandler _handler;

    public UpdateCohortEndpoint(IUpdateCohortHandler handler)
    {
        _handler = handler;
    }

    [HttpPut]
    public async Task<ActionResult<UpdateCohortResultDto>> UpdateAsync(
        Guid cohortId,
        [FromBody] UpdateCohortDto dto,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(userId, cohortId, dto, cancellationToken);
        return Ok(result);
    }
}