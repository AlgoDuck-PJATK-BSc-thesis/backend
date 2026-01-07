using AlgoDuck.Modules.Auth.Commands.ApiKeys.RevokeApiKey;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.ApiKeys.RevokeApiKey;

public sealed class RevokeApiKeyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsApiKeyException()
    {
        var service = new Mock<IApiKeyService>();
        var handler = new RevokeApiKeyHandler(service.Object);

        var ex = await Assert.ThrowsAsync<ApiKeyException>(() =>
            handler.HandleAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenApiKeyIdEmpty_ThrowsApiKeyException()
    {
        var service = new Mock<IApiKeyService>();
        var handler = new RevokeApiKeyHandler(service.Object);

        var ex = await Assert.ThrowsAsync<ApiKeyException>(() =>
            handler.HandleAsync(Guid.NewGuid(), Guid.Empty, CancellationToken.None));

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("API key identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_DelegatesToService()
    {
        var service = new Mock<IApiKeyService>();
        var handler = new RevokeApiKeyHandler(service.Object);

        var userId = Guid.NewGuid();
        var apiKeyId = Guid.NewGuid();

        await handler.HandleAsync(userId, apiKeyId, CancellationToken.None);

        service.Verify(x => x.RevokeApiKeyAsync(userId, apiKeyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}