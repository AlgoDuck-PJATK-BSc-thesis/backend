using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Members.AddCohortMember;

[ApiController]
[Route("api/admin/cohorts/{cohortId:guid}/members")]
[Authorize(Roles = "admin")]
public sealed class AddCohortMemberEndpoint : ControllerBase
{
    private readonly IAddCohortMemberHandler _handler;

    public AddCohortMemberEndpoint(IAddCohortMemberHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync(Guid cohortId, [FromBody] AddCohortMemberDto dto, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(cohortId, dto, cancellationToken);
        return Ok();
    }
}