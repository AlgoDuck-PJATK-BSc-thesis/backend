using System.Reflection;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Interfaces;

public sealed class ICohortRepositoryTests
{
    [Fact]
    public void Interface_HasExpectedMethods()
    {
        var type = typeof(ICohortRepository);

        type.IsInterface.Should().BeTrue();

        type.GetMethod("GetByIdAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("ExistsAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("UserBelongsToCohortAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("GetForUserAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();
    }
}