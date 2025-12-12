using AlgoDuck.Modules.User.Queries.GetUserStatistics;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserStatistics;

public sealed class GetUserStatisticsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenServiceReturnsSummary_ThenMapsToDto()
    {
        var userId = Guid.NewGuid();

        var summary = new StatisticsSummary
        {
            TotalSolvedProblems = 10,
            TotalAttemptedProblems = 20,
            TotalSubmissions = 30,
            AcceptedSubmissions = 11,
            WrongAnswerSubmissions = 12,
            TimeLimitSubmissions = 3,
            RuntimeErrorSubmissions = 4,
            AcceptanceRate = 55.5,
            AverageAttemptsPerSolved = 2.2
        };

        var statsService = new Mock<IStatisticsService>();
        statsService.Setup(x => x.GetStatisticsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var handler = new GetUserStatisticsHandler(statsService.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Equal(10, result.TotalSolvedProblems);
        Assert.Equal(20, result.TotalAttemptedProblems);
        Assert.Equal(30, result.TotalSubmissions);
        Assert.Equal(11, result.AcceptedSubmissions);
        Assert.Equal(12, result.WrongAnswerSubmissions);
        Assert.Equal(3, result.TimeLimitSubmissions);
        Assert.Equal(4, result.RuntimeErrorSubmissions);
        Assert.Equal(55.5, result.AcceptanceRate);
        Assert.Equal(2.2, result.AverageAttemptsPerSolved);
    }
}