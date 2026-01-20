using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Constants;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Services;

public sealed class ProfileServiceTests
{
    private static ApplicationQueryDbContext CreateQueryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    private static ProfileService CreateService(
        out Mock<IUserRepository> userRepositoryMock,
        out Mock<IS3AvatarUrlGenerator> avatarUrlGeneratorMock)
    {
        userRepositoryMock = new Mock<IUserRepository>();
        avatarUrlGeneratorMock = new Mock<IS3AvatarUrlGenerator>();
        var queryContext = CreateQueryContext();

        return new ProfileService(
            userRepositoryMock.Object,
            avatarUrlGeneratorMock.Object,
            queryContext);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var service = CreateService(out var userRepositoryMock, out var avatarUrlGeneratorMock);

        var userId = Guid.NewGuid();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var act = async () => await service.GetProfileAsync(userId, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        avatarUrlGeneratorMock.Verify(g => g.GetAvatarUrl(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserExistsAndNoAvatarSelected_UsesEmptyAvatarKey()
    {
        var service = CreateService(out var userRepositoryMock, out var avatarUrlGeneratorMock);

        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "test-user",
            Email = "test@example.com",
            Coins = 10,
            Experience = 100,
            AmountSolved = 5,
            CohortId = Guid.NewGuid(),
            TwoFactorEnabled = true,
            EmailConfirmed = true,
            UserConfig = new UserConfig
            {
                UserId = userId
            }
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        avatarUrlGeneratorMock
            .Setup(g => g.GetAvatarUrl(string.Empty))
            .Returns(string.Empty);

        var result = await service.GetProfileAsync(userId, CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.Username.Should().Be("test-user");
        result.Email.Should().Be("test@example.com");
        result.Coins.Should().Be(10);
        result.Experience.Should().Be(100);
        result.AmountSolved.Should().Be(5);
        result.CohortId.Should().Be(user.CohortId);
        result.S3AvatarUrl.Should().Be(string.Empty);
        result.TwoFactorEnabled.Should().BeTrue();
        result.EmailConfirmed.Should().BeTrue();

        avatarUrlGeneratorMock.Verify(g => g.GetAvatarUrl(string.Empty), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WhenAvatarKeyEmpty_ThrowsValidationException()
    {
        var service = CreateService(out var userRepositoryMock, out var avatarUrlGeneratorMock);

        var userId = Guid.NewGuid();
        var act = async () => await service.UpdateAvatarAsync(userId, "  ", CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        avatarUrlGeneratorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateUsernameAsync_WhenUsernameTooShort_ThrowsValidationException()
    {
        var service = CreateService(out var userRepositoryMock, out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "old"
        };

        var tooShort = new string('a', ProfileConstants.MinUsernameLength - 1);

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await service.UpdateUsernameAsync(userId, tooShort, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
