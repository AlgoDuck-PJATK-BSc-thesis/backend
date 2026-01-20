namespace AlgoDuck.Modules.User.Queries.User.Stats.GetUserSolvedProblems;

public interface IGetUserSolvedProblemsHandler
{
    Task<IReadOnlyList<UserSolvedProblemsDto>> HandleAsync(
        Guid userId,
        GetUserSolvedProblemsQuery query,
        CancellationToken cancellationToken);
}