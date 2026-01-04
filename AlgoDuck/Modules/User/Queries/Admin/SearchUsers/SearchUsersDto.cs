namespace AlgoDuck.Modules.User.Queries.Admin.SearchUsers;

public sealed class SearchUsersDto
{
    public string? Query { get; init; }

    public int UsernamePage { get; init; } = 1;
    public int UsernamePageSize { get; init; } = 20;

    public int EmailPage { get; init; } = 1;
    public int EmailPageSize { get; init; } = 20;
}