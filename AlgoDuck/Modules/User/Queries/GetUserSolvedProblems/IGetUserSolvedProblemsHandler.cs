namespace AlgoDuck.Modules.User.Queries.GetUserSolvedProblems;

public interface IGetUserSolvedProblemsHandler
{
    Task<IReadOnlyList<UserSolvedProblemsDto>> HandleAsync(
        Guid userId,
        GetUserSolvedProblemsQuery query,
        CancellationToken cancellationToken);
}