namespace AlgoDuck.Modules.User.Queries.GetUserLeaderboardPosition;

public interface IGetUserLeaderboardPositionHandler
{
    Task<UserLeaderboardPositionDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}