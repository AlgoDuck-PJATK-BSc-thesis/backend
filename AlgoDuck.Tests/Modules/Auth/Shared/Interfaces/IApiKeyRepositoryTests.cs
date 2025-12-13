using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Interfaces;

public sealed class IApiKeyRepositoryTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IApiKeyRepository);

        Assert.True(t.IsInterface);

        var add = t.GetMethod("AddAsync");
        Assert.NotNull(add);
        Assert.Equal(typeof(Task), add!.ReturnType);
        Assert.Equal(new[] { typeof(ApiKey), typeof(CancellationToken) }, add.GetParameters().Select(p => p.ParameterType).ToArray());

        var getById = t.GetMethod("GetByIdAsync");
        Assert.NotNull(getById);
        Assert.Equal(typeof(Task<ApiKey?>), getById!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, getById.GetParameters().Select(p => p.ParameterType).ToArray());

        var getUserKeys = t.GetMethod("GetUserApiKeysAsync");
        Assert.NotNull(getUserKeys);
        Assert.Equal(typeof(Task<IReadOnlyList<ApiKey>>), getUserKeys!.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(CancellationToken) }, getUserKeys.GetParameters().Select(p => p.ParameterType).ToArray());

        var findActive = t.GetMethod("FindActiveByPrefixAsync");
        Assert.NotNull(findActive);
        Assert.Equal(typeof(Task<IReadOnlyList<ApiKey>>), findActive!.ReturnType);
        Assert.Equal(new[] { typeof(string), typeof(DateTimeOffset), typeof(CancellationToken) }, findActive.GetParameters().Select(p => p.ParameterType).ToArray());

        var save = t.GetMethod("SaveChangesAsync");
        Assert.NotNull(save);
        Assert.Equal(typeof(Task), save!.ReturnType);
        Assert.Equal(new[] { typeof(CancellationToken) }, save.GetParameters().Select(p => p.ParameterType).ToArray());

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "AddAsync", "FindActiveByPrefixAsync", "GetByIdAsync", "GetUserApiKeysAsync", "SaveChangesAsync" }, all);
    }
}
