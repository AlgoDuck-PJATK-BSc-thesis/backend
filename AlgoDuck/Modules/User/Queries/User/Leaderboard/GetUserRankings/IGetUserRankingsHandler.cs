namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserRankings;

public interface IGetUserRankingsHandler
{
    Task<IReadOnlyList<UserRankingDto>> HandleAsync(GetUserRankingsQuery query, CancellationToken cancellationToken);
}