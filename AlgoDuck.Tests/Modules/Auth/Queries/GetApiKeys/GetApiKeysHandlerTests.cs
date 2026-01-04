using AlgoDuck.Modules.Auth.Queries.ApiKeys.GetApiKeys;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Queries.GetApiKeys;

public sealed class GetApiKeysHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsApiKeyException()
    {
        var apiKeyService = new Mock<IApiKeyService>();
        var handler = new GetApiKeysHandler(apiKeyService.Object);

        var ex = await Assert.ThrowsAsync<ApiKeyException>(() =>
            handler.HandleAsync(Guid.Empty, CancellationToken.None));

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("User identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ReturnsServiceResult()
    {
        var apiKeyService = new Mock<IApiKeyService>();
        var handler = new GetApiKeysHandler(apiKeyService.Object);

        var userId = Guid.NewGuid();
        var expected = new List<ApiKeyDto>
        {
            new ApiKeyDto { Id = Guid.NewGuid(), Name = "k1", Prefix = "p1", CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = null, IsRevoked = false }
        };

        apiKeyService
            .Setup(x => x.GetUserApiKeysAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Same(expected, result);
        apiKeyService.Verify(x => x.GetUserApiKeysAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}