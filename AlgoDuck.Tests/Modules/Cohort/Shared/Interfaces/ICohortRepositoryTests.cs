namespace AlgoDuck.Tests.Modules.Cohort.Shared.Interfaces;

using System.Reflection;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using FluentAssertions;

public sealed class ICohortRepositoryTests
{
    [Fact]
    public void Interface_HasExpectedMethods()
    {
        var type = typeof(ICohortRepository);

        type.IsInterface.Should().BeTrue();

        type.GetMethod("GetByIdAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("AddAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("UpdateAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();
    }
}