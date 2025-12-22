using AlgoDuck.Modules.User.Shared.Utils;
using FluentAssertions;

namespace AlgoDuck.Tests.Modules.User.Shared.Utils;

public sealed class StatisticsCalculatorTests
{
    [Fact]
    public void Calculate_WhenNoSubmissions_SetsRatesToZero()
    {
        var result = StatisticsCalculator.Calculate(
            totalSolved: 0,
            totalAttempted: 0,
            totalSubmissions: 0,
            acceptedSubmissions: 0,
            wrongAnswerSubmissions: 0,
            timeLimitSubmissions: 0,
            runtimeErrorSubmissions: 0);

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
    public void Calculate_WithSubmissions_ComputesAcceptanceRateAndAverageAttempts()
    {
        var result = StatisticsCalculator.Calculate(
            totalSolved: 10,
            totalAttempted: 25,
            totalSubmissions: 40,
            acceptedSubmissions: 18,
            wrongAnswerSubmissions: 15,
            timeLimitSubmissions: 5,
            runtimeErrorSubmissions: 2);

        result.TotalSolvedProblems.Should().Be(10);
        result.TotalAttemptedProblems.Should().Be(25);
        result.TotalSubmissions.Should().Be(40);
        result.AcceptedSubmissions.Should().Be(18);
        result.WrongAnswerSubmissions.Should().Be(15);
        result.TimeLimitSubmissions.Should().Be(5);
        result.RuntimeErrorSubmissions.Should().Be(2);

        result.AcceptanceRate.Should().BeApproximately(18.0 / 40.0, 1e-9);
        result.AverageAttemptsPerSolved.Should().BeApproximately(25.0 / 10.0, 1e-9);
    }

    [Fact]
    public void Calculate_WhenNoSolvedProblems_SetsAverageAttemptsPerSolvedToZero()
    {
        var result = StatisticsCalculator.Calculate(
            totalSolved: 0,
            totalAttempted: 12,
            totalSubmissions: 12,
            acceptedSubmissions: 6,
            wrongAnswerSubmissions: 4,
            timeLimitSubmissions: 1,
            runtimeErrorSubmissions: 1);

        result.TotalSolvedProblems.Should().Be(0);
        result.TotalAttemptedProblems.Should().Be(12);
        result.AverageAttemptsPerSolved.Should().Be(0.0);
        result.AcceptanceRate.Should().BeApproximately(6.0 / 12.0, 1e-9);
    }
}