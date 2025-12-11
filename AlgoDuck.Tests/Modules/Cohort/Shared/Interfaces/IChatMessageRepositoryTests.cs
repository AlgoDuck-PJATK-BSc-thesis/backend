namespace AlgoDuck.Tests.Modules.Cohort.Shared.Interfaces;

using System.Reflection;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using FluentAssertions;

public sealed class IChatMessageRepositoryTests
{
    [Fact]
    public void Interface_HasExpectedMethods()
    {
        var type = typeof(IChatMessageRepository);

        type.IsInterface.Should().BeTrue();

        type.GetMethod("AddAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();

        type.GetMethod("GetMessagesAsync", BindingFlags.Public | BindingFlags.Instance)
            .Should().NotBeNull();
    }
}