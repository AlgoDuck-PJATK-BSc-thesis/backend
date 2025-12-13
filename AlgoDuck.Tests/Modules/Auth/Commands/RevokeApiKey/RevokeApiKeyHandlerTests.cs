using AlgoDuck.Modules.Auth.Commands.RevokeApiKey;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.RevokeApiKey;

public sealed class RevokeApiKeyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsApiKeyException()
    {
        var service = new Mock<IApiKeyService>();
        var handler = new RevokeApiKeyHandler(service.Object, new RevokeApiKeyValidator());

        var ex = await Assert.ThrowsAsync<ApiKeyException>(() =>
            handler.HandleAsync(Guid.Empty, Guid.NewGuid(), new RevokeApiKeyDto(), CancellationToken.None));

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenApiKeyIdEmpty_ThrowsApiKeyException()
    {
        var service = new Mock<IApiKeyService>();
        var handler = new RevokeApiKeyHandler(service.Object, new RevokeApiKeyValidator());

        var ex = await Assert.ThrowsAsync<ApiKeyException>(() =>
            handler.HandleAsync(Guid.NewGuid(), Guid.Empty, new RevokeApiKeyDto(), CancellationToken.None));

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("API key identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_DelegatesToService()
    {
        var service = new Mock<IApiKeyService>();
        var handler = new RevokeApiKeyHandler(service.Object, new RevokeApiKeyValidator());

        var userId = Guid.NewGuid();
        var apiKeyId = Guid.NewGuid();

        await handler.HandleAsync(userId, apiKeyId, new RevokeApiKeyDto(), CancellationToken.None);

        service.Verify(x => x.RevokeApiKeyAsync(userId, apiKeyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}