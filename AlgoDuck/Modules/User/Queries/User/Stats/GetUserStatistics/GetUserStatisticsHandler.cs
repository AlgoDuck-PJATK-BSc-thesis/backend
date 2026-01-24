using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.User.Stats.GetUserStatistics;

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
            Accepted = summary.AcceptedSubmissions,
            WrongAnswer = summary.WrongAnswerSubmissions,
            TimeLimitExceeded = summary.TimeLimitSubmissions,
            RuntimeError = summary.RuntimeErrorSubmissions,
            AcceptanceRate = summary.AcceptanceRate,
            AvgAttemptsPerSolved = summary.AverageAttemptsPerSolved
        };
    }
}