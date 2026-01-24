namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IUserAchievementSyncService
{
    Task EnsureInitializedAsync(Guid userId, CancellationToken cancellationToken);
    Task SyncAsync(Guid userId, CancellationToken cancellationToken);
}