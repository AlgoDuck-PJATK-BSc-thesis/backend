namespace AlgoDuck.Modules.User.Queries.GetUserById;

public sealed class UserDto
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
}