namespace AlgoDuck.Modules.User.Queries.GetSelectedAvatar;

public sealed class SelectedAvatarDto
{
    public Guid UserId { get; init; }
    public string S3AvatarUrl { get; init; } = string.Empty;
}