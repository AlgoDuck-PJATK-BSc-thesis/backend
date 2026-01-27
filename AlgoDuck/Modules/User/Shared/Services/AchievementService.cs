using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class AchievementService : IAchievementService
{
    private readonly ApplicationQueryDbContext _queryDbContext;
    private readonly ApplicationCommandDbContext _commandDbContext;

    public AchievementService(ApplicationQueryDbContext queryDbContext, ApplicationCommandDbContext commandDbContext)
    {
        _queryDbContext = queryDbContext;
        _commandDbContext = commandDbContext;
    }

    public async Task<IReadOnlyList<AchievementProgress>> GetAchievementsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var achievements = await _queryDbContext.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);

        var result = achievements
            .Select(a => new AchievementProgress
            {
                Code = a.AchievementCode,
                Name = a.Achievement.Name,
                Description = a.Achievement.Description,
                CurrentValue = a.CurrentValue,
                TargetValue = a.Achievement.TargetValue,
                IsCompleted = a.IsCompleted,
                CompletedAt = a.CompletedAt
            })
            .ToList();

        return result;
    }

    public async Task AwardAchievement(Guid userId, string achievementCode, CancellationToken cancellationToken = default)
    {
        var exists = await _queryDbContext.UserAchievements
            .AnyAsync(a => a.UserId == userId && a.AchievementCode == achievementCode, cancellationToken);

        if (!exists)
        {
            var achievement = await _commandDbContext.Achievements
                .FirstOrDefaultAsync(a => a.Code == achievementCode, cancellationToken);

            if (achievement == null)
            {
                throw new InvalidOperationException($"Achievement with code '{achievementCode}' not found in catalog.");
            }

            _commandDbContext.UserAchievements.Add(new Models.UserAchievement
            {
                UserId = userId,
                AchievementCode = achievementCode,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
                CurrentValue = achievement.TargetValue,
            });

            await _commandDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
