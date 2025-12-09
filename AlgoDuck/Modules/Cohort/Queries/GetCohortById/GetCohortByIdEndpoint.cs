using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortById;

[ApiController]
[Route("api/cohorts/{cohortId:guid}")]
[Authorize]
public sealed class GetCohortByIdEndpoint : ControllerBase
{
    private readonly IGetCohortByIdHandler _handler;

    public GetCohortByIdEndpoint(IGetCohortByIdHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<ActionResult<GetCohortByIdResultDto>> GetAsync(
        Guid cohortId,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var dto = new GetCohortByIdRequestDto
        {
            CohortId = cohortId
        };

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}