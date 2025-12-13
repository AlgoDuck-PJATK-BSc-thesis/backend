using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Interfaces;

public sealed class IAuthServiceTests
{
    [Fact]
    public void IsInterface_AndCurrentlyEmpty()
    {
        var t = typeof(IAuthService);

        Assert.True(t.IsInterface);
        Assert.Empty(t.GetMethods());
    }
}