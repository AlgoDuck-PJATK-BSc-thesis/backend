namespace AlgoDuck.Modules.User.Queries.GetUserSolvedProblems;

public sealed class GetUserSolvedProblemsQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}