using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class IAuthValidatorTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IAuthValidator);

        Assert.True(t.IsInterface);

        var reg = t.GetMethod("ValidateRegistrationAsync");
        Assert.NotNull(reg);
        Assert.Equal(typeof(Task), reg.ReturnType);
        Assert.Equal(
            new[] { typeof(string), typeof(string), typeof(string), typeof(CancellationToken) },
            reg.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var login = t.GetMethod("ValidateLoginAsync");
        Assert.NotNull(login);
        Assert.Equal(typeof(Task), login.ReturnType);
        Assert.Equal(
            new[] { typeof(string), typeof(string), typeof(CancellationToken) },
            login.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var confirm = t.GetMethod("ValidateEmailConfirmationAsync");
        Assert.NotNull(confirm);
        Assert.Equal(typeof(Task), confirm.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(string), typeof(CancellationToken) },
            confirm.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var change = t.GetMethod("ValidatePasswordChangeAsync");
        Assert.NotNull(change);
        Assert.Equal(typeof(Task), change.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(string), typeof(string), typeof(CancellationToken) },
            change.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "ValidateEmailConfirmationAsync", "ValidateLoginAsync", "ValidatePasswordChangeAsync", "ValidateRegistrationAsync" }, all);
    }
}