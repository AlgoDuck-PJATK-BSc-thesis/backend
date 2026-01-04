using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.DeleteCohort;

[ApiController]
[Route("api/admin/cohorts/{cohortId:guid}")]
[Authorize(Roles = "admin")]
public sealed class DeleteCohortEndpoint : ControllerBase
{
    private readonly IDeleteCohortHandler _handler;

    public DeleteCohortEndpoint(IDeleteCohortHandler handler)
    {
        _handler = handler;
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid cohortId, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(cohortId, cancellationToken);
        return Ok();
    }
}