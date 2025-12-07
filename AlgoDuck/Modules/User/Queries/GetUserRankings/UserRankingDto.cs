namespace AlgoDuck.Modules.User.Queries.GetUserRankings;

public sealed class UserRankingDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public int Rank { get; init; }
    public int Experience { get; init; }
}