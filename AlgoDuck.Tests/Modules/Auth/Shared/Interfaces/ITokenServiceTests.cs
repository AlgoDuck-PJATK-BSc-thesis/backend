using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Interfaces;

public sealed class ITokenServiceTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(ITokenService);

        Assert.True(t.IsInterface);

        var gen = t.GetMethod("GenerateAuthTokensAsync");
        Assert.NotNull(gen);
        Assert.Equal(typeof(Task<AuthResponse>), gen!.ReturnType);
        Assert.Equal(new[] { typeof(ApplicationUser), typeof(CancellationToken) }, gen.GetParameters().Select(p => p.ParameterType).ToArray());

        var refresh = t.GetMethod("RefreshTokensAsync");
        Assert.NotNull(refresh);
        Assert.Equal(typeof(Task<RefreshResult>), refresh!.ReturnType);
        Assert.Equal(new[] { typeof(Session), typeof(CancellationToken) }, refresh.GetParameters().Select(p => p.ParameterType).ToArray());

        var info = t.GetMethod("GetTokenInfoAsync");
        Assert.NotNull(info);
        Assert.Equal(typeof(Task<TokenInfoDto>), info!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, info.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "GenerateAuthTokensAsync", "GetTokenInfoAsync", "RefreshTokensAsync" }, all);
    }
}