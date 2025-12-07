namespace AlgoDuck.Modules.User.Shared.DTOs;

public sealed class UserProfileDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Coins { get; init; }
    public int Experience { get; init; }
    public int AmountSolved { get; init; }
    public Guid? CohortId { get; init; }
    public string Language { get; init; } = string.Empty;
    public string S3AvatarUrl { get; init; } = string.Empty;
    public bool TwoFactorEnabled { get; init; }
    public bool EmailConfirmed { get; init; }
}