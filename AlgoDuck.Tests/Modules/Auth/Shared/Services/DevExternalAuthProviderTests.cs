using AlgoDuck.Modules.Auth.Shared.Services;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public sealed class DevExternalAuthProviderTests
{
    [Fact]
    public async Task GetUserInfoAsync_AlwaysReturnsNull()
    {
        var provider = new DevExternalAuthProvider();

        var result = await provider.GetUserInfoAsync("google", "token", CancellationToken.None);

        Assert.Null(result);
    }
}