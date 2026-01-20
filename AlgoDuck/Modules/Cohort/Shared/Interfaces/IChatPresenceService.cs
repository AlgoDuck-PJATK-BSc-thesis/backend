using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface IChatPresenceService
{
    Task UserConnectedAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken);
    Task UserDisconnectedAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CohortActiveUser>> GetActiveUsersAsync(Guid cohortId, CancellationToken cancellationToken);
    Task<ChatPresenceSnapshot> ReportActivityAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken);
    Task<ChatPresenceSnapshot> GetSnapshotAsync(Guid cohortId, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChatPresenceSnapshot>> GetSnapshotsForCohortAsync(Guid cohortId, CancellationToken cancellationToken);
}