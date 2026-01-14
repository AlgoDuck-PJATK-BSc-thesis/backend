using System.Reflection;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Interfaces;

public sealed class IUserRepositoryTests
{
    [Fact]
    public void Interface_HasExpectedMethodsAndSignatures()
    {
        var type = typeof(IUserRepository);

        type.IsInterface.Should().BeTrue();

        AssertGetByIdAsync(type);
        AssertGetByNameAsync(type);
        AssertGetByEmailAsync(type);
        AssertUpdateAsync(type);
        AssertGetUserSolutionsAsync(type);
        AssertSearchAsync(type);
    }

    static void AssertGetByIdAsync(Type type)
    {
        var method = type.GetMethod("GetByIdAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<ApplicationUser>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertGetByNameAsync(Type type)
    {
        var method = type.GetMethod("GetByNameAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<ApplicationUser>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertGetByEmailAsync(Type type)
    {
        var method = type.GetMethod("GetByEmailAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<ApplicationUser>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertUpdateAsync(Type type)
    {
        var method = type.GetMethod("UpdateAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(ApplicationUser));
        parameters[1].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertGetUserSolutionsAsync(Type type)
    {
        var method = type.GetMethod("GetUserSolutionsAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<IReadOnlyList<UserSolution>>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(4);
        parameters[0].ParameterType.Should().Be(typeof(Guid));
        parameters[1].ParameterType.Should().Be(typeof(int));
        parameters[2].ParameterType.Should().Be(typeof(int));
        parameters[3].ParameterType.Should().Be(typeof(CancellationToken));
    }

    static void AssertSearchAsync(Type type)
    {
        var method = type.GetMethod("SearchAsync", BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull();

        method.ReturnType.Should().Be(typeof(Task<IReadOnlyList<ApplicationUser>>));

        var parameters = method.GetParameters();
        parameters.Should().HaveCount(4);
        parameters[0].ParameterType.Should().Be(typeof(string));
        parameters[1].ParameterType.Should().Be(typeof(int));
        parameters[2].ParameterType.Should().Be(typeof(int));
        parameters[3].ParameterType.Should().Be(typeof(CancellationToken));
    }
}