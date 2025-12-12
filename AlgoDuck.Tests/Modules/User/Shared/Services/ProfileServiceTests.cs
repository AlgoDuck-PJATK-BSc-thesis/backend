using System.Threading;
using System.Threading.Tasks;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Constants;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Services;
using AlgoDuck.Modules.User.Shared.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Shared.Services;

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
        var service = CreateService(
            out var userRepositoryMock,
            out var avatarUrlGeneratorMock);

        var userId = Guid.NewGuid();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var act = async () => await service.GetProfileAsync(userId, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        avatarUrlGeneratorMock.Verify(
            g => g.GetAvatarUrl(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserExistsAndNoAvatarSelected_UsesEmptyAvatarKey()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out var avatarUrlGeneratorMock);

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
                UserId = userId,
                Language = "en"
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
        result.Language.Should().Be("en");
        result.S3AvatarUrl.Should().Be(string.Empty);
        result.TwoFactorEnabled.Should().BeTrue();
        result.EmailConfirmed.Should().BeTrue();

        avatarUrlGeneratorMock.Verify(
            g => g.GetAvatarUrl(string.Empty),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WhenAvatarKeyEmpty_ThrowsValidationException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out var avatarUrlGeneratorMock);

        var userId = Guid.NewGuid();
        var act = async () => await service.UpdateAvatarAsync(userId, "  ", CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        userRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        avatarUrlGeneratorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAvatarAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out var avatarUrlGeneratorMock);

        var userId = Guid.NewGuid();
        const string avatarKey = "some/key.png";

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var act = async () => await service.UpdateAvatarAsync(userId, avatarKey, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();

        avatarUrlGeneratorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAvatarAsync_WhenValid_ThenReturnsSuccess()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out var avatarUrlGeneratorMock);

        var userId = Guid.NewGuid();
        const string avatarKey = "some/key.png";

        var user = new ApplicationUser
        {
            Id = userId
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.UpdateAvatarAsync(userId, avatarKey, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Avatar updated successfully.");

        userRepositoryMock.Verify(
            r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);

        userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Never);

        avatarUrlGeneratorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateUsernameAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        const string newUsername = "newname";

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var act = async () => await service.UpdateUsernameAsync(userId, newUsername, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateUsernameAsync_WhenUsernameEmpty_ThrowsValidationException(string newUsername)
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "old"
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await service.UpdateUsernameAsync(userId, newUsername, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        user.UserName.Should().Be("old");

        userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WhenUsernameTooShort_ThrowsValidationException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

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

        userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WhenUsernameTooLong_ThrowsValidationException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "old"
        };

        var tooLong = new string('a', ProfileConstants.MaxUsernameLength + 1);

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await service.UpdateUsernameAsync(userId, tooLong, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WhenValid_UpdatesUserAndReturnsSuccess()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "old"
        };

        const string newUsername = "newusername";

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.UpdateUsernameAsync(userId, newUsername, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Username updated.");
        user.UserName.Should().Be(newUsername);

        userRepositoryMock.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateLanguageAsync_WhenLanguageEmpty_ThrowsValidationException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await service.UpdateLanguageAsync(userId, "   ", CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateLanguageAsync_WhenLanguageInvalid_ThrowsValidationException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await service.UpdateLanguageAsync(userId, "de", CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateLanguageAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var act = async () => await service.UpdateLanguageAsync(userId, "en", CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task UpdateLanguageAsync_WhenValidAndConfigNull_CreatesConfigAndUpdatesLanguage()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserConfig = null
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.UpdateLanguageAsync(userId, " EN ", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Language updated successfully.");
        user.UserConfig.Should().NotBeNull();
        user.UserConfig!.Language.Should().Be("en");

        userRepositoryMock.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateLanguageAsync_WhenValidAndConfigExists_UpdatesLanguage()
    {
        var service = CreateService(
            out var userRepositoryMock,
            out _);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserConfig = new UserConfig
            {
                UserId = userId,
                Language = "pl"
            }
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.UpdateLanguageAsync(userId, "en", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Language updated successfully.");
        user.UserConfig!.Language.Should().Be("en");

        userRepositoryMock.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void ProfileMapper_ToUserProfileDto_MapsFieldsCorrectly()
    {
        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "user",
            Email = "user@example.com",
            Coins = 42,
            Experience = 1000,
            AmountSolved = 7,
            CohortId = cohortId,
            UserConfig = new UserConfig
            {
                UserId = userId,
                Language = "pl"
            }
        };

        var dto = ProfileMapper.ToUserProfileDto(user);

        dto.UserId.Should().Be(userId);
        dto.Username.Should().Be("user");
        dto.Email.Should().Be("user@example.com");
        dto.Coins.Should().Be(42);
        dto.Experience.Should().Be(1000);
        dto.AmountSolved.Should().Be(7);
        dto.CohortId.Should().Be(cohortId);
        dto.Language.Should().Be("pl");
        dto.S3AvatarUrl.Should().BeEmpty();
    }
}