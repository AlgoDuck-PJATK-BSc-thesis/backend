using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class IExternalAuthProviderTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IExternalAuthProvider);

        Assert.True(t.IsInterface);

        var get = t.GetMethod("GetUserInfoAsync");
        Assert.NotNull(get);
        Assert.Equal(typeof(Task<AuthUserDto?>), get!.ReturnType);
        Assert.Equal(
            new[] { typeof(string), typeof(string), typeof(CancellationToken) },
            get.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var all = t.GetMethods().Select(m => m.Name).ToArray();
        Assert.Equal(new[] { "GetUserInfoAsync" }, all);
    }
}