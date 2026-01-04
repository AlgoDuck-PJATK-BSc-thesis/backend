namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetCohortLeaderboard;

public sealed class GetCohortLeaderboardRequestDto
{
    public Guid CohortId { get; set; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}