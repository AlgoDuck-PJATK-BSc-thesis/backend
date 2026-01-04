using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;

[ApiController]
[Route("api/admin/cohorts/{cohortId:guid}")]
[Authorize(Roles = "admin")]
public sealed class UpdateCohortEndpoint : ControllerBase
{
    private readonly IUpdateCohortHandler _handler;

    public UpdateCohortEndpoint(IUpdateCohortHandler handler)
    {
        _handler = handler;
    }

    [HttpPut]
    public async Task<IActionResult> Put(Guid cohortId, [FromBody] UpdateCohortDto dto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(cohortId, dto, cancellationToken);
        return Ok(result);
    }
}