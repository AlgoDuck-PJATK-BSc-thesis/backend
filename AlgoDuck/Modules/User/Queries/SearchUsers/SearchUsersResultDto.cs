namespace AlgoDuck.Modules.User.Queries.SearchUsers;

public sealed class SearchUsersResultDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}