namespace AlgoDuck.Modules.User.Queries.AdminShared;

public sealed class AdminUserItemDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}