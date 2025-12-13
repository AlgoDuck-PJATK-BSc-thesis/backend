using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Interfaces;

public sealed class IEmailTransportTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IEmailTransport);

        Assert.True(t.IsInterface);

        var send = t.GetMethod("SendAsync");
        Assert.NotNull(send);
        Assert.Equal(typeof(Task), send!.ReturnType);

        var ps = send.GetParameters().Select(p => p.ParameterType).ToArray();
        Assert.Equal(new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(CancellationToken) }, ps);

        var optional = send.GetParameters().Select(p => p.HasDefaultValue).ToArray();
        Assert.Equal(new[] { false, false, false, true, true }, optional);

        var all = t.GetMethods().Select(m => m.Name).ToArray();
        Assert.Equal(new[] { "SendAsync" }, all);
    }
}