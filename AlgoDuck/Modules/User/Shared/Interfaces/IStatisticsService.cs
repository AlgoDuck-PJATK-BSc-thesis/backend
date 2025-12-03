using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsSummary> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken);
}