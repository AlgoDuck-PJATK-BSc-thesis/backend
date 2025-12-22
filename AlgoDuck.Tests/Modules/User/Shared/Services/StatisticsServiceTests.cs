using System.Threading;
using System.Threading.Tasks;
using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Modules.User.Shared.Services;

public sealed class StatisticsServiceTests
{
    private static ApplicationQueryDbContext CreateQueryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    [Fact]
    public async Task GetStatisticsAsync_WhenNoSolutions_ReturnsZeroedStatistics()
    {
        using var context = CreateQueryContext();
        var service = new StatisticsService(context);
        var userId = Guid.NewGuid();

        var result = await service.GetStatisticsAsync(userId, CancellationToken.None);

        result.TotalSolvedProblems.Should().Be(0);
        result.TotalAttemptedProblems.Should().Be(0);
        result.TotalSubmissions.Should().Be(0);
        result.AcceptedSubmissions.Should().Be(0);
        result.WrongAnswerSubmissions.Should().Be(0);
        result.TimeLimitSubmissions.Should().Be(0);
        result.RuntimeErrorSubmissions.Should().Be(0);
        result.AcceptanceRate.Should().Be(0.0);
        result.AverageAttemptsPerSolved.Should().Be(0.0);
    }

    [Fact]
    public async Task GetSolvedProblemsAsync_WhenNoSolutions_ReturnsEmptyList()
    {
        using var context = CreateQueryContext();
        var service = new StatisticsService(context);
        var userId = Guid.NewGuid();

        var result = await service.GetSolvedProblemsAsync(userId, page: 1, pageSize: 10, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void StatisticsSummary_CanBeInitialized()
    {
        var summary = new StatisticsSummary
        {
            TotalSolvedProblems = 1,
            TotalAttemptedProblems = 2,
            TotalSubmissions = 3,
            AcceptedSubmissions = 1,
            WrongAnswerSubmissions = 1,
            TimeLimitSubmissions = 0,
            RuntimeErrorSubmissions = 1,
            AcceptanceRate = 1.0 / 3.0,
            AverageAttemptsPerSolved = 2.0
        };

        summary.TotalSolvedProblems.Should().Be(1);
        summary.TotalAttemptedProblems.Should().Be(2);
        summary.TotalSubmissions.Should().Be(3);
        summary.AcceptedSubmissions.Should().Be(1);
        summary.WrongAnswerSubmissions.Should().Be(1);
        summary.TimeLimitSubmissions.Should().Be(0);
        summary.RuntimeErrorSubmissions.Should().Be(1);
        summary.AcceptanceRate.Should().BeApproximately(1.0 / 3.0, 1e-6);
        summary.AverageAttemptsPerSolved.Should().Be(2.0);
    }
}