namespace AlgoDuck.Modules.Item.Queries.GetUserPreviewData;

public class UserPreviewDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required Guid SelectedAvatar { get; set; }
    public required long ItemCreatedCount { get; set; }
}