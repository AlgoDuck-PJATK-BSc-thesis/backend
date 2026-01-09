using System.Reflection;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Interfaces;

public sealed class IProfileServiceTests
{
    [Fact]
    public void Interface_HasExpectedMethodsAndSignatures()
    {
        var type = typeof(IProfileService);

        type.IsInterface.Should().BeTrue();

        AssertGetProfileAsync(type);
        AssertUpdateAvatarAsync(type);
        AssertUpdateUsernameAsync(type);
        AssertUpdateLanguageAsync(type);
    }

    static void AssertGetProfileAsync(Type type)
    {
        var method = type.GetMethod("GetProfileAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method!.ReturnType.Should().Be(typeof(Task<UserProfileDto>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertUpdateAvatarAsync(Type type)
    {
        var method = type.GetMethod("UpdateAvatarAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method!.ReturnType.Should().Be(typeof(Task<ProfileUpdateResult>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(string));
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertUpdateUsernameAsync(Type type)
    {
        var method = type.GetMethod("UpdateUsernameAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method!.ReturnType.Should().Be(typeof(Task<ProfileUpdateResult>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(string));
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertUpdateLanguageAsync(Type type)
    {
        var method = type.GetMethod("UpdateLanguageAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method!.ReturnType.Should().Be(typeof(Task<ProfileUpdateResult>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(string));
        parameters[2].ParameterType.Should().Be(typeof(CancellationToken));
    }
}