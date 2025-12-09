using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/messages")]
[Authorize]
public sealed class GetCohortMessagesEndpoint : ControllerBase
{
    private readonly IGetCohortMessagesHandler _handler;

    public GetCohortMessagesEndpoint(IGetCohortMessagesHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<ActionResult<GetCohortMessagesResultDto>> GetAsync(
        Guid cohortId,
        [FromQuery] DateTime? beforeCreatedAt,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = cohortId,
            BeforeCreatedAt = beforeCreatedAt,
            PageSize = pageSize
        };

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}