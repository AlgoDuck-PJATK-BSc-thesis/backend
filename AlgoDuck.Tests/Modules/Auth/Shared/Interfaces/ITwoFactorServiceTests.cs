using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Interfaces;

public sealed class ITwoFactorServiceTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(ITwoFactorService);

        Assert.True(t.IsInterface);

        var send = t.GetMethod("SendLoginCodeAsync");
        Assert.NotNull(send);
        Assert.Equal(typeof(Task<(string challengeId, DateTimeOffset expiresAt)>), send!.ReturnType);
        Assert.Equal(new[] { typeof(ApplicationUser), typeof(CancellationToken) }, send.GetParameters().Select(p => p.ParameterType).ToArray());

        var verify = t.GetMethod("VerifyLoginCodeAsync");
        Assert.NotNull(verify);
        Assert.Equal(typeof(Task<(bool ok, Guid userId, string? error)>), verify!.ReturnType);
        Assert.Equal(new[] { typeof(string), typeof(string), typeof(CancellationToken) }, verify.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "SendLoginCodeAsync", "VerifyLoginCodeAsync" }, all);
    }
}