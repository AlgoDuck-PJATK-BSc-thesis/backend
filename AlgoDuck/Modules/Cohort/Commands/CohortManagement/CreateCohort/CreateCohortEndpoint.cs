using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;

[ApiController]
[Route("api/cohorts")]
[Authorize]
public sealed class CreateCohortEndpoint : ControllerBase
{
    private readonly ICreateCohortHandler _handler;

    public CreateCohortEndpoint(ICreateCohortHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<ActionResult<CreateCohortResultDto>> CreateAsync(
        [FromBody] CreateCohortDto dto,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}