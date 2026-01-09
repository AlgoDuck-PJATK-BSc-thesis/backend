using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class IPermissionsServiceTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IPermissionsService);

        Assert.True(t.IsInterface);

        var ensure = t.GetMethod("EnsureUserHasPermissionAsync");
        Assert.NotNull(ensure);
        Assert.Equal(typeof(Task), ensure!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(string), typeof(CancellationToken) }, ensure.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).ToArray();
        Assert.Equal(new[] { "EnsureUserHasPermissionAsync" }, all);
    }
}