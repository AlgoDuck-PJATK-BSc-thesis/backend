using System.Security.Claims;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class IPermissionsRepositoryTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IPermissionsRepository);

        Assert.True(t.IsInterface);

        var perms = t.GetMethod("GetUserPermissionsAsync");
        Assert.NotNull(perms);
        Assert.Equal(typeof(Task<IReadOnlyList<string>>), perms.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, perms.GetParameters().Select(p => p.ParameterType).ToArray());

        var claims = t.GetMethod("GetUserClaimsAsync");
        Assert.NotNull(claims);
        Assert.Equal(typeof(Task<IReadOnlyList<Claim>>), claims.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, claims.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "GetUserClaimsAsync", "GetUserPermissionsAsync" }, all);
    }
}