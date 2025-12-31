namespace AlgoDuck.Modules.User.Queries.AdminGetUsers;

public sealed class AdminGetUsersDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}