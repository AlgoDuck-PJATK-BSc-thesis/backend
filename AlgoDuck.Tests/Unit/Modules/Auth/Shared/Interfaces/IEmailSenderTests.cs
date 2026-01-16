using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class IEmailSenderTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IEmailSender);

        Assert.True(t.IsInterface);

        var confirm = t.GetMethod("SendEmailConfirmationAsync");
        Assert.NotNull(confirm);
        Assert.Equal(typeof(Task), confirm.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(string), typeof(string), typeof(CancellationToken) },
            confirm.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var reset = t.GetMethod("SendPasswordResetAsync");
        Assert.NotNull(reset);
        Assert.Equal(typeof(Task), reset.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(string), typeof(string), typeof(CancellationToken) },
            reset.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var code = t.GetMethod("SendTwoFactorCodeAsync");
        Assert.NotNull(code);
        Assert.Equal(typeof(Task), code.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(string), typeof(string), typeof(CancellationToken) },
            code.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var change = t.GetMethod("SendEmailChangeConfirmationAsync");
        Assert.NotNull(change);
        Assert.Equal(typeof(Task), change.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(string), typeof(string), typeof(CancellationToken) },
            change.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "SendEmailChangeConfirmationAsync", "SendEmailConfirmationAsync", "SendPasswordResetAsync", "SendTwoFactorCodeAsync" }, all);
    }
}