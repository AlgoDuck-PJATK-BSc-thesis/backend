using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Settings.GetUserConfig;

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
    public async Task HandleAsync_WhenUserHasNoConfig_ThenReturnsDefaultsAndWeeklyReminders()
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
        Assert.False(result.EmailNotificationsEnabled);
        Assert.Equal("u1", result.Username);
        Assert.Equal("u1@test.local", result.Email);
        Assert.Equal(string.Empty, result.S3AvatarUrl);

        Assert.NotNull(result.WeeklyReminders);
        Assert.Equal(7, result.WeeklyReminders.Count);

        foreach (var reminder in result.WeeklyReminders)
        {
            Assert.False(reminder.Enabled);
            Assert.Equal(8, reminder.Hour);
            Assert.Equal(0, reminder.Minute);
        }
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasConfig_ThenMapsConfigValuesAndReminders()
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
                    EmailNotificationsEnabled = true,
                    ReminderMonHour = 9,
                    ReminderWedHour = 19,
                    User = null!
                }
            });

        var handler = new GetUserConfigHandler(userRepository.Object);

        var result = await handler.HandleAsync(new GetUserConfigRequestDto { UserId = userId }, CancellationToken.None);

        Assert.True(result.IsDarkMode);
        Assert.True(result.IsHighContrast);
        Assert.True(result.EmailNotificationsEnabled);

        Assert.Equal(7, result.WeeklyReminders.Count);

        var mon = result.WeeklyReminders.Single(r => r.Day == "Mon");
        Assert.True(mon.Enabled);
        Assert.Equal(9, mon.Hour);

        var wed = result.WeeklyReminders.Single(r => r.Day == "Wed");
        Assert.True(wed.Enabled);
        Assert.Equal(19, wed.Hour);

        var tue = result.WeeklyReminders.Single(r => r.Day == "Tue");
        Assert.False(tue.Enabled);
    }
}
