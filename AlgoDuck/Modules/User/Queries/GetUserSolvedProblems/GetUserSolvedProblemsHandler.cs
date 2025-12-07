using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserSolvedProblems;

public sealed class GetUserSolvedProblemsHandler : IGetUserSolvedProblemsHandler
{
    private readonly IStatisticsService _statisticsService;

    public GetUserSolvedProblemsHandler(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    public async Task<IReadOnlyList<UserSolvedProblemsDto>> HandleAsync(
        Guid userId,
        GetUserSolvedProblemsQuery query,
        CancellationToken cancellationToken)
    {
        var solved = await _statisticsService.GetSolvedProblemsAsync(
            userId,
            query.Page,
            query.PageSize,
            cancellationToken);

        return solved
            .Select(s => new UserSolvedProblemsDto
            {
                ProblemId = s.ProblemId
            })
            .ToList();
    }
}