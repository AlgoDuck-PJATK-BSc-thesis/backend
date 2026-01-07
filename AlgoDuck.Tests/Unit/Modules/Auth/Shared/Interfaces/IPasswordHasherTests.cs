using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class IPasswordHasherTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IPasswordHasher);

        Assert.True(t.IsInterface);

        var hash = t.GetMethod("HashPassword");
        Assert.NotNull(hash);
        Assert.Equal(typeof(string), hash!.ReturnType);
        Assert.Equal(new[] { typeof(string) }, hash.GetParameters().Select(p => p.ParameterType).ToArray());

        var verify = t.GetMethod("VerifyHashedPassword");
        Assert.NotNull(verify);
        Assert.Equal(typeof(bool), verify!.ReturnType);
        Assert.Equal(new[] { typeof(string), typeof(string) }, verify.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "HashPassword", "VerifyHashedPassword" }, all);
    }
}