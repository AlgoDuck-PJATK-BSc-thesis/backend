using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Interfaces;

public sealed class IApiKeyServiceTests
{
    [Fact]
    public void HasExpectedMethods()
    {
        var t = typeof(IApiKeyService);

        Assert.True(t.IsInterface);

        var create = t.GetMethod("CreateApiKeyAsync");
        Assert.NotNull(create);
        Assert.Equal(typeof(Task<ApiKeyCreationResult>), create.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(string), typeof(TimeSpan?), typeof(CancellationToken) },
            create.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var getUser = t.GetMethod("GetUserApiKeysAsync");
        Assert.NotNull(getUser);
        Assert.Equal(typeof(Task<IReadOnlyList<ApiKeyDto>>), getUser.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(CancellationToken) },
            getUser.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var revoke = t.GetMethod("RevokeApiKeyAsync");
        Assert.NotNull(revoke);
        Assert.Equal(typeof(Task), revoke.ReturnType);
        Assert.Equal(
            new[] { typeof(Guid), typeof(Guid), typeof(CancellationToken) },
            revoke.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var validate = t.GetMethod("ValidateAndGetUserIdAsync");
        Assert.NotNull(validate);
        Assert.Equal(typeof(Task<Guid>), validate.ReturnType);
        Assert.Equal(
            new[] { typeof(string), typeof(CancellationToken) },
            validate.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        var all = t.GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "CreateApiKeyAsync", "GetUserApiKeysAsync", "RevokeApiKeyAsync", "ValidateAndGetUserIdAsync" }, all);
    }
}