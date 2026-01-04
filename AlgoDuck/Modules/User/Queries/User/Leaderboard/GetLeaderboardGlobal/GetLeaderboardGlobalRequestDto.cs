namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetLeaderboardGlobal;

public sealed class GetLeaderboardGlobalRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}