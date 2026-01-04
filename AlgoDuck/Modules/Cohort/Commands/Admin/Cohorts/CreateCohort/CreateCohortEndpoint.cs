using AlgoDuck.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

[ApiController]
[Route("api/admin/cohorts")]
[Authorize(Roles = "admin")]
public sealed class CreateCohortEndpoint : ControllerBase
{
    private readonly ICreateCohortHandler _handler;

    public CreateCohortEndpoint(ICreateCohortHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateCohortDto dto, CancellationToken cancellationToken)
    {
        var adminUserId = User.GetUserId();
        var result = await _handler.HandleAsync(adminUserId, dto, cancellationToken);
        return Ok(result);
    }
}