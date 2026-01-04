using AlgoDuck.Modules.User.Queries.User.Stats.GetUserSolvedProblems;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserSolvedProblems;

public sealed class GetUserSolvedProblemsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenServiceReturnsSolvedProblems_ThenMapsToDtos()
    {
        var userId = Guid.NewGuid();

        var solved = new List<SolvedProblemSummary>
        {
            new() { ProblemId = Guid.NewGuid() },
            new() { ProblemId = Guid.NewGuid() }
        }.AsReadOnly();

        var statsService = new Mock<IStatisticsService>();
        statsService.Setup(x => x.GetSolvedProblemsAsync(userId, 2, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solved);

        var handler = new GetUserSolvedProblemsHandler(statsService.Object);

        var result = await handler.HandleAsync(userId, new GetUserSolvedProblemsQuery
        {
            Page = 2,
            PageSize = 50
        }, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(solved[0].ProblemId, result[0].ProblemId);
        Assert.Equal(solved[1].ProblemId, result[1].ProblemId);
    }
}