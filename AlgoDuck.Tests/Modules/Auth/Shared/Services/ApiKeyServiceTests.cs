using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using AlgoDuck.Modules.Auth.Shared.Validators;
using AlgoDuck.Shared.Utilities;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public class ApiKeyServiceTests
{
    [Fact]
    public async Task CreateApiKeyAsync_WhenUserIdIsEmpty_ThenThrowsApiKeyException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);

        await Assert.ThrowsAsync<ApiKeyException>(() =>
            service.CreateApiKeyAsync(Guid.Empty, "my-key", TimeSpan.FromDays(30), CancellationToken.None));

        apiKeyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()), Times.Never);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateApiKeyAsync_WhenUserDoesNotExist_ThenThrowsApiKeyException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();
        var name = "my-api-key";

        authRepositoryMock
            .Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser)null);

        await Assert.ThrowsAsync<ApiKeyException>(() =>
            service.CreateApiKeyAsync(userId, name, TimeSpan.FromDays(30), CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        apiKeyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()), Times.Never);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateApiKeyAsync_WhenUserExists_ThenCreatesAndReturnsApiKey()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();
        var name = "alice-api-key";
        var lifetime = TimeSpan.FromDays(30);
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = "alice@gmail.com"
        };
        ApiKey captured = null;

        authRepositoryMock
            .Setup(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        apiKeyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()))
            .Callback<ApiKey, CancellationToken>((a, _) => captured = a)
            .Returns(Task.CompletedTask);

        apiKeyRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await service.CreateApiKeyAsync(userId, name, lifetime, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.ApiKey);
        Assert.False(string.IsNullOrWhiteSpace(result.RawKey));
        Assert.Equal(name, result.ApiKey.Name);

        Assert.NotNull(captured);
        Assert.Equal(userId, captured.UserId);
        Assert.Equal(name, captured.Name);
        Assert.False(string.IsNullOrWhiteSpace(captured.Prefix));
        Assert.False(string.IsNullOrWhiteSpace(captured.KeyHash));
        Assert.False(string.IsNullOrWhiteSpace(captured.KeySalt));
        Assert.True(captured.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.NotNull(captured.ExpiresAt);

        authRepositoryMock.Verify(x => x.FindByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        apiKeyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ApiKey>(), It.IsAny<CancellationToken>()), Times.Once);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserApiKeysAsync_WhenUserIdIsEmpty_ThenThrowsApiKeyException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);

        await Assert.ThrowsAsync<ApiKeyException>(() =>
            service.GetUserApiKeysAsync(Guid.Empty, CancellationToken.None));

        apiKeyRepositoryMock.Verify(x => x.GetUserApiKeysAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserApiKeysAsync_WhenUserIdIsValid_ThenReturnsMappedDtos()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();

        var keys = new List<ApiKey>
        {
            new ApiKey { Id = Guid.NewGuid(), UserId = userId, Name = "key1", Prefix = "p1", CreatedAt = DateTimeOffset.UtcNow },
            new ApiKey { Id = Guid.NewGuid(), UserId = userId, Name = "key2", Prefix = "p2", CreatedAt = DateTimeOffset.UtcNow }
        };

        apiKeyRepositoryMock
            .Setup(x => x.GetUserApiKeysAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(keys);

        var result = await service.GetUserApiKeysAsync(userId, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "key1");
        Assert.Contains(result, x => x.Name == "key2");

        apiKeyRepositoryMock.Verify(x => x.GetUserApiKeysAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeApiKeyAsync_WhenUserIdIsEmpty_ThenThrowsApiKeyException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);

        await Assert.ThrowsAsync<ApiKeyException>(() =>
            service.RevokeApiKeyAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None));

        apiKeyRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeApiKeyAsync_WhenApiKeyDoesNotExist_ThenThrowsApiKeyException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();
        var apiKeyId = Guid.NewGuid();

        apiKeyRepositoryMock
            .Setup(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiKey)null);

        await Assert.ThrowsAsync<ApiKeyException>(() =>
            service.RevokeApiKeyAsync(userId, apiKeyId, CancellationToken.None));

        apiKeyRepositoryMock.Verify(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()), Times.Once);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeApiKeyAsync_WhenApiKeyBelongsToAnotherUser_ThenThrowsPermissionException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var apiKeyId = Guid.NewGuid();

        var apiKey = new ApiKey
        {
            Id = apiKeyId,
            UserId = otherUserId
        };

        apiKeyRepositoryMock
            .Setup(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);

        await Assert.ThrowsAsync<PermissionException>(() =>
            service.RevokeApiKeyAsync(userId, apiKeyId, CancellationToken.None));

        apiKeyRepositoryMock.Verify(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()), Times.Once);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeApiKeyAsync_WhenApiKeyAlreadyRevoked_ThenDoesNotSaveChanges()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();
        var apiKeyId = Guid.NewGuid();
        var revokedAt = DateTimeOffset.UtcNow.AddMinutes(-5);

        var apiKey = new ApiKey
        {
            Id = apiKeyId,
            UserId = userId,
            RevokedAt = revokedAt
        };

        apiKeyRepositoryMock
            .Setup(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);

        await service.RevokeApiKeyAsync(userId, apiKeyId, CancellationToken.None);

        Assert.Equal(revokedAt, apiKey.RevokedAt);
        apiKeyRepositoryMock.Verify(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()), Times.Once);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeApiKeyAsync_WhenApiKeyIsActive_ThenSetsRevokedAtAndSavesChanges()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();
        var apiKeyId = Guid.NewGuid();

        var apiKey = new ApiKey
        {
            Id = apiKeyId,
            UserId = userId,
            RevokedAt = null
        };

        apiKeyRepositoryMock
            .Setup(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);

        apiKeyRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await service.RevokeApiKeyAsync(userId, apiKeyId, CancellationToken.None);

        Assert.NotNull(apiKey.RevokedAt);
        apiKeyRepositoryMock.Verify(x => x.GetByIdAsync(apiKeyId, It.IsAny<CancellationToken>()), Times.Once);
        apiKeyRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAndGetUserIdAsync_WhenRawKeyIsEmpty_ThenThrowsApiKeyException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);

        await Assert.ThrowsAsync<ApiKeyException>(() =>
            service.ValidateAndGetUserIdAsync("", CancellationToken.None));

        apiKeyRepositoryMock.Verify(x => x.FindActiveByPrefixAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAndGetUserIdAsync_WhenNoCandidates_ThenThrowsApiKeyException()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var rawKey = "raw-key-value";

        apiKeyRepositoryMock
            .Setup(x => x.FindActiveByPrefixAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ApiKey>());

        await Assert.ThrowsAsync<ApiKeyException>(() =>
            service.ValidateAndGetUserIdAsync(rawKey, CancellationToken.None));

        apiKeyRepositoryMock.Verify(x => x.FindActiveByPrefixAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAndGetUserIdAsync_WhenCandidateMatchesHash_ThenReturnsUserId()
    {
        var apiKeyRepositoryMock = new Mock<IApiKeyRepository>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new ApiKeyValidator();

        var service = new ApiKeyService(apiKeyRepositoryMock.Object, authRepositoryMock.Object, validator);
        var userId = Guid.NewGuid();
        var rawKey = "raw-key-value";
        var saltBytes = System.Text.Encoding.UTF8.GetBytes("salt-for-tests");
        var saltB64 = Convert.ToBase64String(saltBytes);
        var hashB64 = HashingHelper.HashPassword(rawKey, saltBytes);

        var candidate = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            KeySalt = saltB64,
            KeyHash = hashB64
        };

        apiKeyRepositoryMock
            .Setup(x => x.FindActiveByPrefixAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApiKey> { candidate });

        var resultUserId = await service.ValidateAndGetUserIdAsync(rawKey, CancellationToken.None);

        Assert.Equal(userId, resultUserId);

        apiKeyRepositoryMock.Verify(x => x.FindActiveByPrefixAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
