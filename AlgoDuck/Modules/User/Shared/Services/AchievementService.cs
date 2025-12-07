using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class AchievementService : IAchievementService
{
    private readonly ApplicationQueryDbContext _queryDbContext;

    public AchievementService(ApplicationQueryDbContext queryDbContext)
    {
        _queryDbContext = queryDbContext;
    }

    public async Task<IReadOnlyList<AchievementProgress>> GetAchievementsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var achievements = await _queryDbContext.UserAchievements
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);

        var result = achievements
            .Select(a => new AchievementProgress
            {
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                CurrentValue = a.CurrentValue,
                TargetValue = a.TargetValue,
                IsCompleted = a.IsCompleted
            })
            .ToList();

        return result;
    }
}