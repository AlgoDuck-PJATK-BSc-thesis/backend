using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
                IsCompleted = a.IsCompleted,
                CompletedAt = a.CompletedAt
            })
            .ToList();

        return result;
    }

    public async Task AwardAchievement(Guid userId, string achievementCode, CancellationToken cancellationToken = default)
    {
        var exists = await _queryDbContext.UserAchievements
            .AnyAsync(a => a.UserId == userId && a.Code == achievementCode, cancellationToken);

        if (!exists)
        {
            var name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(achievementCode.Replace("-", " "));
            _commandDbContext.UserAchievements.Add(new Models.UserAchievement
            {
                UserId = userId,
                Code = achievementCode,
                Name = name,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
                CurrentValue = 1,
                TargetValue = 1
            });
            await _commandDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
