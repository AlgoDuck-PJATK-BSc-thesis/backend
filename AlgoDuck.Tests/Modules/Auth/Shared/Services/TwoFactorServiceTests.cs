using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public class TwoFactorServiceTests
{
    IMemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    ApplicationUser CreateUser(string email = "alice@gmail.com")
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = email
        };
    }

    [Fact]
    public async Task SendLoginCodeAsync_WhenUserHasNoEmail_ThenThrowsTwoFactorException()
    {
        var cache = CreateMemoryCache();
        var emailSenderMock = new Mock<IEmailSender>();
        var loggerMock = new Mock<ILogger<TwoFactorService>>();
        var service = new TwoFactorService(cache, emailSenderMock.Object, loggerMock.Object);
        var user = CreateUser(email: "");

        await Assert.ThrowsAsync<TwoFactorException>(() =>
            service.SendLoginCodeAsync(user, CancellationToken.None));

        emailSenderMock.Verify(x => x.SendTwoFactorCodeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendLoginCodeAsync_WhenUserIsValid_ThenStoresChallengeAndSendsEmail()
    {
        var cache = CreateMemoryCache();
        var emailSenderMock = new Mock<IEmailSender>();
        var loggerMock = new Mock<ILogger<TwoFactorService>>();
        var service = new TwoFactorService(cache, emailSenderMock.Object, loggerMock.Object);
        var user = CreateUser();
        string capturedCode = null;

        emailSenderMock
            .Setup(x => x.SendTwoFactorCodeAsync(user.Id, user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, CancellationToken>((_, _, code, _) => capturedCode = code)
            .Returns(Task.CompletedTask);

        var (challengeId, expiresAt) = await service.SendLoginCodeAsync(user, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(challengeId));
        Assert.True(expiresAt > DateTimeOffset.UtcNow);
        Assert.NotNull(capturedCode);
        Assert.Equal(6, capturedCode.Length);
        Assert.All(capturedCode, c => Assert.InRange(c, '0', '9'));

        emailSenderMock.Verify(x => x.SendTwoFactorCodeAsync(user.Id, user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyLoginCodeAsync_WhenChallengeDoesNotExist_ThenReturnsChallengeNotFound()
    {
        var cache = CreateMemoryCache();
        var emailSenderMock = new Mock<IEmailSender>();
        var loggerMock = new Mock<ILogger<TwoFactorService>>();
        var service = new TwoFactorService(cache, emailSenderMock.Object, loggerMock.Object);

        var result = await service.VerifyLoginCodeAsync("nonexistent", "123456", CancellationToken.None);

        Assert.False(result.ok);
        Assert.Equal(Guid.Empty, result.userId);
        Assert.Equal("challenge_not_found", result.error);
    }

    [Fact]
    public async Task VerifyLoginCodeAsync_WhenCodeIsIncorrect_ThenReturnsInvalidCodeAndIncrementsAttempts()
    {
        var cache = CreateMemoryCache();
        var emailSenderMock = new Mock<IEmailSender>();
        var loggerMock = new Mock<ILogger<TwoFactorService>>();
        var service = new TwoFactorService(cache, emailSenderMock.Object, loggerMock.Object);
        var user = CreateUser();
        string sentCode = null;

        emailSenderMock
            .Setup(x => x.SendTwoFactorCodeAsync(user.Id, user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, CancellationToken>((_, _, code, _) => sentCode = code)
            .Returns(Task.CompletedTask);

        var (challengeId, _) = await service.SendLoginCodeAsync(user, CancellationToken.None);

        var wrongCode = sentCode == "000000" ? "000001" : "000000";

        var result = await service.VerifyLoginCodeAsync(challengeId, wrongCode, CancellationToken.None);

        Assert.False(result.ok);
        Assert.Equal(Guid.Empty, result.userId);
        Assert.Equal("invalid_code", result.error);
    }

    [Fact]
    public async Task VerifyLoginCodeAsync_WhenCodeIsCorrect_ThenReturnsOkAndUserIdAndRemovesChallenge()
    {
        var cache = CreateMemoryCache();
        var emailSenderMock = new Mock<IEmailSender>();
        var loggerMock = new Mock<ILogger<TwoFactorService>>();
        var service = new TwoFactorService(cache, emailSenderMock.Object, loggerMock.Object);
        var user = CreateUser();
        string sentCode = null;

        emailSenderMock
            .Setup(x => x.SendTwoFactorCodeAsync(user.Id, user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, CancellationToken>((_, _, code, _) => sentCode = code)
            .Returns(Task.CompletedTask);

        var (challengeId, _) = await service.SendLoginCodeAsync(user, CancellationToken.None);

        var firstResult = await service.VerifyLoginCodeAsync(challengeId, sentCode, CancellationToken.None);

        Assert.True(firstResult.ok);
        Assert.Equal(user.Id, firstResult.userId);
        Assert.Null(firstResult.error);

        var secondResult = await service.VerifyLoginCodeAsync(challengeId, sentCode, CancellationToken.None);

        Assert.False(secondResult.ok);
        Assert.Equal(Guid.Empty, secondResult.userId);
        Assert.Equal("challenge_not_found", secondResult.error);
    }

    [Fact]
    public async Task VerifyLoginCodeAsync_WhenMaxAttemptsExceeded_ThenChallengeIsRemoved()
    {
        var cache = CreateMemoryCache();
        var emailSenderMock = new Mock<IEmailSender>();
        var loggerMock = new Mock<ILogger<TwoFactorService>>();
        var service = new TwoFactorService(cache, emailSenderMock.Object, loggerMock.Object);
        var user = CreateUser();
        string sentCode = null;

        emailSenderMock
            .Setup(x => x.SendTwoFactorCodeAsync(user.Id, user.Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, CancellationToken>((_, _, code, _) => sentCode = code)
            .Returns(Task.CompletedTask);

        var (challengeId, _) = await service.SendLoginCodeAsync(user, CancellationToken.None);

        var wrongCode = sentCode == "000000" ? "000001" : "000000";

        for (var i = 0; i < 5; i++)
        {
            var attempt = await service.VerifyLoginCodeAsync(challengeId, wrongCode, CancellationToken.None);
            Assert.False(attempt.ok);
            Assert.Equal("invalid_code", attempt.error);
        }

        var finalResult = await service.VerifyLoginCodeAsync(challengeId, wrongCode, CancellationToken.None);

        Assert.False(finalResult.ok);
        Assert.Equal(Guid.Empty, finalResult.userId);
        Assert.Equal("challenge_not_found", finalResult.error);
    }
}
