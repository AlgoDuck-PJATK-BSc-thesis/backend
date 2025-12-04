using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserAchievements;

public sealed class GetUserAchievementsHandler : IGetUserAchievementsHandler
{
    private readonly IAchievementService _achievementService;

    public GetUserAchievementsHandler(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    public async Task<IReadOnlyList<UserAchievementDto>> HandleAsync(Guid userId, GetUserAchievementsQuery query, CancellationToken cancellationToken)
    {
        var achievements = await _achievementService.GetAchievementsAsync(userId, cancellationToken);

        if (query.Completed.HasValue)
        {
            achievements = achievements
                .Where(a => a.IsCompleted == query.Completed.Value)
                .ToList()
                .AsReadOnly();
        }

        if (!string.IsNullOrWhiteSpace(query.CodeFilter))
        {
            var filter = query.CodeFilter.ToLowerInvariant();
            achievements = achievements
                .Where(a => a.Code.ToLowerInvariant().Contains(filter))
                .ToList()
                .AsReadOnly();
        }

        var skip = (query.Page - 1) * query.PageSize;

        var paged = achievements
            .Skip(skip)
            .Take(query.PageSize)
            .Select(a => new UserAchievementDto
            {
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                CurrentValue = a.CurrentValue,
                TargetValue = a.TargetValue,
                IsCompleted = a.IsCompleted,
                CompletedAt = null
            })
            .ToList()
            .AsReadOnly();

        return paged;
    }
}