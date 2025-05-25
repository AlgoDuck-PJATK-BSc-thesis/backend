namespace WebApplication1.Modules.CohortModule.DTOs;

public class CohortChatDto
{
    public required Guid CohortId { get; set; }
    public required Guid UserId { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Username { get; set; }
    public string? UserProfilePicture { get; set; }
}