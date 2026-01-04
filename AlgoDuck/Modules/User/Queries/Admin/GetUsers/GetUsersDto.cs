namespace AlgoDuck.Modules.User.Queries.Admin.GetUsers;

public sealed class GetUsersDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}