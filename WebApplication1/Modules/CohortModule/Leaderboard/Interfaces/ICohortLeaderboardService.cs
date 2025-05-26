using WebApplication1.Modules.CohortModule.Leaderboard.DTOs;

namespace WebApplication1.Modules.CohortModule.Leaderboard.Interfaces;

public interface ICohortLeaderboardService
{
    Task<List<CohortLeaderboardDto>> GetLeaderboardAsync(Guid cohortId);
}