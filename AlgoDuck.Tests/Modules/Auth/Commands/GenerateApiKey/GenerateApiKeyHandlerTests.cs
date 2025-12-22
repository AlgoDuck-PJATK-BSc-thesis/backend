using AlgoDuck.Modules.Auth.Commands.GenerateApiKey;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.GenerateApiKey;

public sealed class GenerateApiKeyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var apiKeyService = new Mock<IApiKeyService>();
        var handler = new GenerateApiKeyHandler(apiKeyService.Object, new GenerateApiKeyValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new GenerateApiKeyDto { Name = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsApiKeyException()
    {
        var apiKeyService = new Mock<IApiKeyService>();
        var handler = new GenerateApiKeyHandler(apiKeyService.Object, new GenerateApiKeyValidator());

        var ex = await Assert.ThrowsAsync<ApiKeyException>(() =>
            handler.HandleAsync(Guid.Empty, new GenerateApiKeyDto { Name = "key" }, CancellationToken.None));

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);

        apiKeyService.Verify(
            x => x.CreateApiKeyAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenLifetimeDaysNull_PassesNullLifetimeToService()
    {
        var apiKeyService = new Mock<IApiKeyService>();

        var creation = new ApiKeyCreationResult
        {
            ApiKey = new ApiKeyDto { Id = Guid.NewGuid(), Name = "key", CreatedAt = DateTimeOffset.UtcNow },
            RawKey = "raw"
        };

        apiKeyService
            .Setup(x => x.CreateApiKeyAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(creation);

        var handler = new GenerateApiKeyHandler(apiKeyService.Object, new GenerateApiKeyValidator());

        var userId = Guid.NewGuid();
        var result = await handler.HandleAsync(userId, new GenerateApiKeyDto { Name = "key", LifetimeDays = null }, CancellationToken.None);

        Assert.Same(creation.ApiKey, result.ApiKey);
        Assert.Equal("raw", result.RawKey);

        apiKeyService.Verify(
            x => x.CreateApiKeyAsync(userId, "key", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenLifetimeDaysProvided_PassesDaysAsTimeSpanToService()
    {
        var apiKeyService = new Mock<IApiKeyService>();

        var creation = new ApiKeyCreationResult
        {
            ApiKey = new ApiKeyDto { Id = Guid.NewGuid(), Name = "key", CreatedAt = DateTimeOffset.UtcNow },
            RawKey = "raw"
        };

        apiKeyService
            .Setup(x => x.CreateApiKeyAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(creation);

        var handler = new GenerateApiKeyHandler(apiKeyService.Object, new GenerateApiKeyValidator());

        var userId = Guid.NewGuid();
        var days = 10;

        var result = await handler.HandleAsync(
            userId,
            new GenerateApiKeyDto { Name = "key", LifetimeDays = days },
            CancellationToken.None);

        Assert.Same(creation.ApiKey, result.ApiKey);
        Assert.Equal("raw", result.RawKey);

        apiKeyService.Verify(
            x => x.CreateApiKeyAsync(userId, "key", TimeSpan.FromDays(days), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
