using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.User.Activity.GetUserAchievements;

public sealed class GetUserAchievementsHandler : IGetUserAchievementsHandler
{
    private readonly IAchievementService _achievementService;
    private readonly IUserAchievementSyncService _achievementSyncService;

    public GetUserAchievementsHandler(
        IAchievementService achievementService,
        IUserAchievementSyncService achievementSyncService)
    {
        _achievementService = achievementService;
        _achievementSyncService = achievementSyncService;
    }

    public async Task<IReadOnlyList<UserAchievementDto>> HandleAsync(Guid userId, GetUserAchievementsRequestDto requestDto, CancellationToken cancellationToken)
    {
        await _achievementSyncService.SyncAsync(userId, cancellationToken);

        var achievements = await _achievementService.GetAchievementsAsync(userId, cancellationToken);

        if (requestDto.Completed.HasValue)
        {
            achievements = achievements
                .Where(a => a.IsCompleted == requestDto.Completed.Value)
                .ToList()
                .AsReadOnly();
        }

        if (!string.IsNullOrWhiteSpace(requestDto.CodeFilter))
        {
            var filter = requestDto.CodeFilter.ToLowerInvariant();
            achievements = achievements
                .Where(a => a.Code.ToLowerInvariant().Contains(filter))
                .ToList()
                .AsReadOnly();
        }

        var skip = (requestDto.Page - 1) * requestDto.PageSize;

        var paged = achievements
            .Skip(skip)
            .Take(requestDto.PageSize)
            .Select(a => new UserAchievementDto
            {
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                CurrentValue = a.CurrentValue,
                TargetValue = a.TargetValue,
                IsCompleted = a.IsCompleted,
                CompletedAt = a.IsCompleted ? a.CompletedAt : null
            })
            .ToList()
            .AsReadOnly();

        return paged;
    }
}
