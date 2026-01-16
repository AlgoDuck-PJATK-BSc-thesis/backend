using System.Reflection;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Interfaces;

public sealed class Is3AvatarUrlGeneratorTests
{
    [Fact]
    public void Interface_HasExpectedMethodsAndSignatures()
    {
        var type = typeof(IS3AvatarUrlGenerator);

        type.IsInterface.Should().BeTrue();

        var method = type.GetMethod("GetAvatarUrl", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(string));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be(typeof(string));
    }
}