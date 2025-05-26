using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Modules.CohortModule.Leaderboard.DTOs;
using WebApplication1.Modules.CohortModule.Leaderboard.Interfaces;

namespace WebApplication1.Modules.CohortModule.Leaderboard.Controllers;

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