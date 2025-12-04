namespace AlgoDuck.Modules.User.Queries.GetUserRankings;

public interface IGetUserRankingsHandler
{
    Task<IReadOnlyList<UserRankingDto>> HandleAsync(GetUserRankingsQuery query, CancellationToken cancellationToken);
}