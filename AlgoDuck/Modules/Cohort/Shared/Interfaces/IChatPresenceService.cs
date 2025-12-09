using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface IChatPresenceService
{
    Task UserConnectedAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken);
    Task UserDisconnectedAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CohortActiveUser>> GetActiveUsersAsync(Guid cohortId, CancellationToken cancellationToken);
}