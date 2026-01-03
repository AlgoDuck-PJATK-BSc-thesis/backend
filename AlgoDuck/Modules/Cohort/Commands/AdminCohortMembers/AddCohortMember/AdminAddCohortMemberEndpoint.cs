using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.AddCohortMember;

[ApiController]
[Route("api/admin/cohorts/{cohortId:guid}/members")]
[Authorize(Roles = "admin")]
public sealed class AdminAddCohortMemberEndpoint : ControllerBase
{
    private readonly IAdminAddCohortMemberHandler _handler;

    public AdminAddCohortMemberEndpoint(IAdminAddCohortMemberHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync(Guid cohortId, [FromBody] AdminAddCohortMemberDto dto, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(cohortId, dto, cancellationToken);
        return Ok();
    }
}