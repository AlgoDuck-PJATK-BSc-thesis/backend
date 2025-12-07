namespace AlgoDuck.Modules.User.Queries.GetUserActivity;

public sealed class GetUserActivityRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}