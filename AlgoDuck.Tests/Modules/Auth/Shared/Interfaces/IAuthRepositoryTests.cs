using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Interfaces;

public sealed class IAuthRepositoryTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IAuthRepository);

        Assert.True(t.IsInterface);

        var byEmail = t.GetMethod("FindByEmailAsync");
        Assert.NotNull(byEmail);
        Assert.Equal(typeof(Task<ApplicationUser?>), byEmail!.ReturnType);
        Assert.Equal(new[] { typeof(string), typeof(CancellationToken) }, byEmail.GetParameters().Select(p => p.ParameterType).ToArray());

        var byId = t.GetMethod("FindByIdAsync");
        Assert.NotNull(byId);
        Assert.Equal(typeof(Task<ApplicationUser?>), byId!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, byId.GetParameters().Select(p => p.ParameterType).ToArray());

        var byUserName = t.GetMethod("FindByUserNameAsync");
        Assert.NotNull(byUserName);
        Assert.Equal(typeof(Task<ApplicationUser?>), byUserName!.ReturnType);
        Assert.Equal(new[] { typeof(string), typeof(CancellationToken) }, byUserName.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "FindByEmailAsync", "FindByIdAsync", "FindByUserNameAsync" }, all);
    }
}