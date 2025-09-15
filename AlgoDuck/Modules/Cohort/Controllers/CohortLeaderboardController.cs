using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Controllers;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/leaderboard")]
[Authorize]
public class CohortLeaderboardController : ControllerBase
{
    private readonly ICohortLeaderboardService _leaderboardService;

    public CohortLeaderboardController(ICohortLeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CohortLeaderboardDto>>> GetLeaderboard(Guid cohortId)
    {
        var leaderboard = await _leaderboardService.GetLeaderboardAsync(cohortId);
        return Ok(leaderboard);
    }
}