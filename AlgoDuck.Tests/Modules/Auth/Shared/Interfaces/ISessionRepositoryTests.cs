using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Interfaces;

public sealed class ISessionRepositoryTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(ISessionRepository);

        Assert.True(t.IsInterface);

        var add = t.GetMethod("AddAsync");
        Assert.NotNull(add);
        Assert.Equal(typeof(Task), add!.ReturnType);
        Assert.Equal(new[] { typeof(Session), typeof(CancellationToken) }, add.GetParameters().Select(p => p.ParameterType).ToArray());

        var get = t.GetMethod("GetByIdAsync");
        Assert.NotNull(get);
        Assert.Equal(typeof(Task<Session?>), get!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, get.GetParameters().Select(p => p.ParameterType).ToArray());

        var list = t.GetMethod("GetUserSessionsAsync");
        Assert.NotNull(list);
        Assert.Equal(typeof(Task<IReadOnlyList<Session>>), list!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, list.GetParameters().Select(p => p.ParameterType).ToArray());

        var save = t.GetMethod("SaveChangesAsync");
        Assert.NotNull(save);
        Assert.Equal(typeof(Task), save!.ReturnType);
        Assert.Equal(new[] { typeof(CancellationToken) }, save.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "AddAsync", "GetByIdAsync", "GetUserSessionsAsync", "SaveChangesAsync" }, all);
    }
}