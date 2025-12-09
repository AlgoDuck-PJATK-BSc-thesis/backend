using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/members")]
[Authorize]
public sealed class GetCohortMembersEndpoint : ControllerBase
{
    private readonly IGetCohortMembersHandler _handler;

    public GetCohortMembersEndpoint(IGetCohortMembersHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<ActionResult<GetCohortMembersResultDto>> GetAsync(
        Guid cohortId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var dto = new GetCohortMembersRequestDto
        {
            CohortId = cohortId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}