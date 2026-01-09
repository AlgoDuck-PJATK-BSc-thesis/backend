using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class ITokenRepositoryTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(ITokenRepository);

        Assert.True(t.IsInterface);

        var info = t.GetMethod("GetTokenInfoAsync");
        Assert.NotNull(info);
        Assert.Equal(typeof(Task<TokenInfoDto?>), info!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, info.GetParameters().Select(p => p.ParameterType).ToArray());

        var list = t.GetMethod("GetUserTokensAsync");
        Assert.NotNull(list);
        Assert.Equal(typeof(Task<IReadOnlyList<TokenInfoDto>>), list!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, list.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "GetTokenInfoAsync", "GetUserTokensAsync" }, all);
    }
}