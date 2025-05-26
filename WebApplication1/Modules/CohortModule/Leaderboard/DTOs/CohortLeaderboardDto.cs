namespace WebApplication1.Modules.CohortModule.Leaderboard.DTOs;

public class CohortLeaderboardDto
{
    public Guid UserId { get; set; }
    public required string Username { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public int Experience { get; set; }
    public int Rank { get; set; }
}