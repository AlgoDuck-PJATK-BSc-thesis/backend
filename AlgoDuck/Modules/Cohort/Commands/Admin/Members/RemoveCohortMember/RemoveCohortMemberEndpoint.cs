using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Members.RemoveCohortMember;

[ApiController]
[Route("api/admin/cohorts/{cohortId:guid}/members")]
[Authorize(Roles = "admin")]
public sealed class RemoveCohortMemberEndpoint : ControllerBase
{
    private readonly IRemoveCohortMemberHandler _handler;

    public RemoveCohortMemberEndpoint(IRemoveCohortMemberHandler handler)
    {
        _handler = handler;
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> RemoveAsync(Guid cohortId, Guid userId, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(cohortId, userId, cancellationToken);
        return Ok();
    }
}