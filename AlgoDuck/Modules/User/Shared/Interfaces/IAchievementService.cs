using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IAchievementService
{
    Task<IReadOnlyList<AchievementProgress>> GetAchievementsAsync(Guid userId, CancellationToken cancellationToken);
}