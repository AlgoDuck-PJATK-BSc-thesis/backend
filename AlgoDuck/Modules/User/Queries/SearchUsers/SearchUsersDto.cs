namespace AlgoDuck.Modules.User.Queries.SearchUsers;

public sealed class SearchUsersDto
{
    public string Query { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}