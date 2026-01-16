using System.Reflection;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Interfaces;

public sealed class IStatisticsServiceTests
{
    [Fact]
    public void Interface_HasExpectedMethodsAndSignatures()
    {
        var type = typeof(IStatisticsService);

        type.IsInterface.Should().BeTrue();

        AssertGetStatisticsAsync(type);
        AssertGetSolvedProblemsAsync(type);
    }

    static void AssertGetStatisticsAsync(Type type)
    {
        var method = type.GetMethod("GetStatisticsAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<StatisticsSummary>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertGetSolvedProblemsAsync(Type type)
    {
        var method = type.GetMethod("GetSolvedProblemsAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<IReadOnlyList<SolvedProblemSummary>>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(4);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(int));
        parameters[2].ParameterType.Should().Be(typeof(int));
        parameters[3].ParameterType.Should().Be(typeof(CancellationToken));
    }
}