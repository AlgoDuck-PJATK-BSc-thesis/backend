namespace AlgoDuck.Modules.Cohort.DTOs;

public class CohortLeaderboardDto
{
    public Guid UserId { get; set; }
    public required string Username { get; set; } = string.Empty;
    public int Experience { get; set; }
    public int Rank { get; set; }
}