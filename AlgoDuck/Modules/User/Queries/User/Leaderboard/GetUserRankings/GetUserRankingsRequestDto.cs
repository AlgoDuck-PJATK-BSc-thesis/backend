namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserRankings;

public sealed class GetUserRankingsQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}