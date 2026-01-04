namespace AlgoDuck.Modules.User.Queries.User.Activity.GetUserAchievements;

public interface IGetUserAchievementsHandler
{
    Task<IReadOnlyList<UserAchievementDto>> HandleAsync(Guid userId, GetUserAchievementsRequestDto requestDto, CancellationToken cancellationToken);
}