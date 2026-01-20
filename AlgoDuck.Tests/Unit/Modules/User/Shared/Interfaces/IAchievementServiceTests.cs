using System.Reflection;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Interfaces;

public sealed class IAchievementServiceTests
{
    [Fact]
    public void Interface_HasExpectedMethodsAndSignatures()
    {
        var type = typeof(IAchievementService);

        type.IsInterface.Should().BeTrue();

        var method = type.GetMethod("GetAchievementsAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<IReadOnlyList<AchievementProgress>>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }
}