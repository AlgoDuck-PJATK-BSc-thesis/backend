namespace AlgoDuck.Modules.User.Queries.GetUserAchievements;

public interface IGetUserAchievementsHandler
{
    Task<IReadOnlyList<UserAchievementDto>> HandleAsync(Guid userId, GetUserAchievementsRequestDto requestDto, CancellationToken cancellationToken);
}