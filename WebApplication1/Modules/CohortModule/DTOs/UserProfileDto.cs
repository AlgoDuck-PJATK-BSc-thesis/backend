namespace WebApplication1.Modules.CohortModule.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string? ProfilePicture { get; set; }
    public int Experience { get; set; }
}