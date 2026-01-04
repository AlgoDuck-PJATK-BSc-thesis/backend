namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserLeaderboardPosition;

public interface IGetUserLeaderboardPositionHandler
{
    Task<UserLeaderboardPositionDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}