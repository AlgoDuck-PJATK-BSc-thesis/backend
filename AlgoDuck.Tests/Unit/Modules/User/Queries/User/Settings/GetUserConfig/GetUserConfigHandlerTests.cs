using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Settings.GetUserConfig;

public sealed class GetUserConfigHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        await using var dbContext = CreateCommandDbContext();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserConfigHandler(userRepository.Object, dbContext);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(new GetUserConfigRequestDto { UserId = Guid.NewGuid() }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoConfig_ThenReturnsDefaultsAndWeeklyReminders()
    {
        await using var dbContext = CreateCommandDbContext();

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

        var handler = new GetUserConfigHandler(userRepository.Object, dbContext);

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
    public async Task HandleAsync_WhenUserHasConfig_ThenMapsConfigValuesAndRemindersFromJoinTable()
    {
        await using var dbContext = CreateCommandDbContext();

        await SeedStudyRemindersAsync(dbContext);

        var userId = Guid.NewGuid();

        var dbUser = new ApplicationUser
        {
            Id = userId,
            UserName = "u1",
            Email = "u1@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        dbContext.Users.Add(dbUser);
        await dbContext.SaveChangesAsync();

        await SeedUserSetReminderRowAsync(dbContext, userId, 1, 9);
        await SeedUserSetReminderRowAsync(dbContext, userId, 3, 19);

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
                    User = new ApplicationUser { Id = userId }
                }
            });

        var handler = new GetUserConfigHandler(userRepository.Object, dbContext);

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

    static ApplicationCommandDbContext CreateCommandDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    static async Task SeedStudyRemindersAsync(ApplicationCommandDbContext dbContext)
    {
        var set = dbContext.Set<StudyReminder>();
        var exists = await set.AnyAsync();
        if (exists)
        {
            return;
        }

        set.AddRange(
            new StudyReminder { StudyReminderId = 1, Code = "MON", DayOfWeek = 1, Name = "Monday" },
            new StudyReminder { StudyReminderId = 2, Code = "TUE", DayOfWeek = 2, Name = "Tuesday" },
            new StudyReminder { StudyReminderId = 3, Code = "WED", DayOfWeek = 3, Name = "Wednesday" },
            new StudyReminder { StudyReminderId = 4, Code = "THU", DayOfWeek = 4, Name = "Thursday" },
            new StudyReminder { StudyReminderId = 5, Code = "FRI", DayOfWeek = 5, Name = "Friday" },
            new StudyReminder { StudyReminderId = 6, Code = "SAT", DayOfWeek = 6, Name = "Saturday" },
            new StudyReminder { StudyReminderId = 7, Code = "SUN", DayOfWeek = 7, Name = "Sunday" }
        );

        await dbContext.SaveChangesAsync();
    }

    static async Task SeedUserSetReminderRowAsync(ApplicationCommandDbContext dbContext, Guid userId, short studyReminderId, short? hour)
    {
        var joinSet = dbContext.Set<UserSetStudyReminder>();

        var existing = await joinSet
            .Where(e => e.UserId == userId && e.StudyReminderId == studyReminderId)
            .ToListAsync();

        if (existing.Count > 0)
        {
            existing[0].Hour = hour;
        }
        else
        {
            joinSet.Add(new UserSetStudyReminder
            {
                UserId = userId,
                StudyReminderId = studyReminderId,
                Hour = hour
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
