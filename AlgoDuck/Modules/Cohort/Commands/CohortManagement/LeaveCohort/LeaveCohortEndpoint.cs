using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.LeaveCohort;

[ApiController]
[Route("api/cohorts/leave")]
[Authorize]
public sealed class LeaveCohortEndpoint : ControllerBase
{
    private readonly ILeaveCohortHandler _handler;

    public LeaveCohortEndpoint(ILeaveCohortHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> LeaveAsync(
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        await _handler.HandleAsync(userId, cancellationToken);
        return NoContent();
    }
}