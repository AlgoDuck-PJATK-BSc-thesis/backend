namespace AlgoDuck.Modules.User.Shared.DTOs;

public sealed class UserLeaderboardEntryDto
{
    public int Rank { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public int Experience { get; init; }
    public int AmountSolved { get; init; }
    public Guid? CohortId { get; init; }
}