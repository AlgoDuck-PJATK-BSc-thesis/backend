using System.Reflection;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Interfaces;

public sealed class IChatMessageRepositoryTests
{
    [Fact]
    public void Interface_HasExpectedMethods()
    {
        var type = typeof(IChatMessageRepository);

        type.IsInterface.Should().BeTrue();

        type.GetMethod("AddAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("GetPagedForCohortAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("SoftDeleteAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();
    }
}