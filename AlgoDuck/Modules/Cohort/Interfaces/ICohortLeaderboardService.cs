using AlgoDuck.Modules.Cohort.DTOs;

namespace AlgoDuck.Modules.Cohort.Interfaces;

public interface ICohortLeaderboardService
{
    Task<List<CohortLeaderboardDto>> GetLeaderboardAsync(Guid cohortId);
}