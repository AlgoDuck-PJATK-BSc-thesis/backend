namespace AlgoDuck.Modules.User.Queries.GetUserLeaderboardPosition;

public sealed class UserLeaderboardPositionDto
{
    public Guid UserId { get; init; }
    public int Rank { get; init; }
    public int TotalUsers { get; init; }
    public int Experience { get; init; }
    public int AmountSolved { get; init; }
    public double Percentile { get; init; }
}