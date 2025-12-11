using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortActiveMembers;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/active-members")]
[Authorize]
public sealed class GetCohortActiveMembersEndpoint : ControllerBase
{
    private readonly IGetCohortActiveMembersHandler _handler;

    public GetCohortActiveMembersEndpoint(IGetCohortActiveMembersHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<ActionResult<GetCohortActiveMembersResultDto>> GetAsync(
        Guid cohortId,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var dto = new GetCohortActiveMembersRequestDto
        {
            CohortId = cohortId
        };

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}