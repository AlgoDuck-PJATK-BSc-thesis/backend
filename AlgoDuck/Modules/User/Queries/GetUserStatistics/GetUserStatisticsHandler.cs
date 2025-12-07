using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserStatistics;

public sealed class GetUserStatisticsHandler : IGetUserStatisticsHandler
{
    private readonly IStatisticsService _statisticsService;

    public GetUserStatisticsHandler(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    public async Task<UserStatisticsDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var summary = await _statisticsService.GetStatisticsAsync(userId, cancellationToken);

        return new UserStatisticsDto
        {
            TotalSolvedProblems = summary.TotalSolvedProblems,
            TotalAttemptedProblems = summary.TotalAttemptedProblems,
            TotalSubmissions = summary.TotalSubmissions,
            AcceptedSubmissions = summary.AcceptedSubmissions,
            WrongAnswerSubmissions = summary.WrongAnswerSubmissions,
            TimeLimitSubmissions = summary.TimeLimitSubmissions,
            RuntimeErrorSubmissions = summary.RuntimeErrorSubmissions,
            AcceptanceRate = summary.AcceptanceRate,
            AverageAttemptsPerSolved = summary.AverageAttemptsPerSolved
        };
    }
}