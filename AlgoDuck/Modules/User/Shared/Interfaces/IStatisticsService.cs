using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsSummary> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SolvedProblemSummary>> GetSolvedProblemsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}