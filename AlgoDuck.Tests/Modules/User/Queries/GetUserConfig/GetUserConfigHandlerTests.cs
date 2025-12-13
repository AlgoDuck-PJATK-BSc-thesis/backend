using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.GetUserConfig;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserConfig;

public sealed class GetUserConfigHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserConfigHandler(userRepository.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(new GetUserConfigRequestDto { UserId = Guid.NewGuid() }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoConfig_ThenReturnsDefaults()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                UserName = "u1",
                Email = "u1@test.local",
                SecurityStamp = Guid.NewGuid().ToString(),
                UserConfig = null
            });

        var handler = new GetUserConfigHandler(userRepository.Object);

        var result = await handler.HandleAsync(new GetUserConfigRequestDto { UserId = userId }, CancellationToken.None);

        Assert.False(result.IsDarkMode);
        Assert.False(result.IsHighContrast);
        Assert.Equal(string.Empty, result.Language);
        Assert.Equal(string.Empty, result.S3AvatarUrl);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasConfig_ThenMapsConfigValues()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                UserName = "u1",
                Email = "u1@test.local",
                SecurityStamp = Guid.NewGuid().ToString(),
                UserConfig = new UserConfig
                {
                    UserId = userId,
                    IsDarkMode = true,
                    IsHighContrast = true,
                    Language = "pl",
                    EmailNotificationsEnabled = true,
                    PushNotificationsEnabled = false,
                    User = null!
                }
            });

        var handler = new GetUserConfigHandler(userRepository.Object);

        var result = await handler.HandleAsync(new GetUserConfigRequestDto { UserId = userId }, CancellationToken.None);

        Assert.True(result.IsDarkMode);
        Assert.True(result.IsHighContrast);
        Assert.Equal("pl", result.Language);
        Assert.Equal(string.Empty, result.S3AvatarUrl);
    }
}