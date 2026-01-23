using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Reminders;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.User.Preferences.UpdatePreferences;

public sealed class UpdatePreferencesHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        await using var dbContext = CreateCommandDbContext();

        var dto = new UpdatePreferencesDto
        {
            IsDarkMode = true,
            IsHighContrast = true,
            EmailNotificationsEnabled = true,
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Mon", Enabled = true, Hour = 8, Minute = 0 }
            }
        };

        var validator = CreateValidatorMock(dto);
        var calculator = new ReminderNextAtCalculator();
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object, calculator);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserMissing_ThenThrowsUserNotFoundException()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        var dto = new UpdatePreferencesDto
        {
            IsDarkMode = true,
            IsHighContrast = true,
            EmailNotificationsEnabled = true
        };

        var validator = CreateValidatorMock(dto);
        var calculator = new ReminderNextAtCalculator();
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object, calculator);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserExistsButConfigMissing_ThenCreatesConfigAndUpdates()
    {
        await using var dbContext = CreateCommandDbContext();

        await SeedStudyRemindersAsync(dbContext);

        var userId = Guid.NewGuid();
        SeedUser(dbContext, userId);

        var dto = new UpdatePreferencesDto
        {
            IsDarkMode = false,
            IsHighContrast = true,
            EmailNotificationsEnabled = false
        };

        var validator = CreateValidatorMock(dto);
        var calculator = new ReminderNextAtCalculator();
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object, calculator);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        var cfg = await dbContext.UserConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.NotNull(cfg);
        Assert.False(cfg.IsDarkMode);
        Assert.True(cfg.IsHighContrast);
        Assert.False(cfg.EmailNotificationsEnabled);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenUpdatesUserConfig_AndWritesJoinTableHours_AndComputesNextAt()
    {
        await using var dbContext = CreateCommandDbContext();

        await SeedStudyRemindersAsync(dbContext);

        var userId = Guid.NewGuid();
        SeedUser(dbContext, userId);
        var config = SeedUserConfig(dbContext, userId);

        var dto = new UpdatePreferencesDto
        {
            IsDarkMode = true,
            IsHighContrast = true,
            EmailNotificationsEnabled = true,
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Mon", Enabled = true, Hour = 9, Minute = 0 },
                new() { Day = "Wed", Enabled = true, Hour = 19, Minute = 0 },
                new() { Day = "Sun", Enabled = false, Hour = 10, Minute = 0 }
            }
        };

        var validator = CreateValidatorMock(dto);
        var calculator = new ReminderNextAtCalculator();
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object, calculator);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.True(config.IsDarkMode);
        Assert.True(config.IsHighContrast);
        Assert.True(config.EmailNotificationsEnabled);

        var hours = await GetUserSetReminderHoursAsync(dbContext, userId);

        Assert.Equal(7, hours.Count);

        Assert.True(hours.TryGetValue(1, out var monHour));
        Assert.True(monHour.HasValue);
        Assert.Equal(9, monHour.Value);

        Assert.True(hours.TryGetValue(2, out var tueHour));
        Assert.Null(tueHour);

        Assert.True(hours.TryGetValue(3, out var wedHour));
        Assert.True(wedHour.HasValue);
        Assert.Equal(19, wedHour.Value);

        Assert.True(hours.TryGetValue(4, out var thuHour));
        Assert.Null(thuHour);

        Assert.True(hours.TryGetValue(5, out var friHour));
        Assert.Null(friHour);

        Assert.True(hours.TryGetValue(6, out var satHour));
        Assert.Null(satHour);

        Assert.True(hours.TryGetValue(7, out var sunHour));
        Assert.Null(sunHour);

        Assert.True(config.StudyReminderNextAtUtc.HasValue);
        Assert.True(config.StudyReminderNextAtUtc.Value > DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task HandleAsync_WhenEmailNotificationsDisabled_ThenClearsNextReminderAtUtc()
    {
        await using var dbContext = CreateCommandDbContext();

        await SeedStudyRemindersAsync(dbContext);

        var userId = Guid.NewGuid();
        SeedUser(dbContext, userId);
        var config = SeedUserConfig(dbContext, userId);

        config.StudyReminderNextAtUtc = DateTimeOffset.UtcNow.AddHours(1);
        dbContext.SaveChanges();

        var dto = new UpdatePreferencesDto
        {
            IsDarkMode = false,
            IsHighContrast = false,
            EmailNotificationsEnabled = false,
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Mon", Enabled = true, Hour = 8, Minute = 0 }
            }
        };

        var validator = CreateValidatorMock(dto);
        var calculator = new ReminderNextAtCalculator();
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object, calculator);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Null(config.StudyReminderNextAtUtc);
    }

    [Fact]
    public async Task HandleAsync_WhenRemindersEnabledButWeeklyRemindersEmpty_ThenClearsAllJoinHours()
    {
        await using var dbContext = CreateCommandDbContext();

        await SeedStudyRemindersAsync(dbContext);

        var userId = Guid.NewGuid();
        SeedUser(dbContext, userId);
        var config = SeedUserConfig(dbContext, userId);

        await SeedUserSetReminderRowAsync(dbContext, userId, 1, 9);
        await SeedUserSetReminderRowAsync(dbContext, userId, 3, 19);

        config.EmailNotificationsEnabled = true;
        config.StudyReminderNextAtUtc = DateTimeOffset.UtcNow.AddHours(2);
        dbContext.SaveChanges();

        var dto = new UpdatePreferencesDto
        {
            IsDarkMode = true,
            IsHighContrast = false,
            EmailNotificationsEnabled = true,
            WeeklyReminders = new List<Reminder>()
        };

        var validator = CreateValidatorMock(dto);
        var calculator = new ReminderNextAtCalculator();
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object, calculator);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        var hours = await GetUserSetReminderHoursAsync(dbContext, userId);

        Assert.Equal(7, hours.Count);
        Assert.All(hours.Values, v => Assert.Null(v));

        Assert.True(config.StudyReminderNextAtUtc.HasValue);
    }

    static ApplicationCommandDbContext CreateCommandDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    static void SeedUser(ApplicationCommandDbContext dbContext, Guid userId)
    {
        dbContext.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "test-user",
            Email = "test@example.com",
            EmailConfirmed = true
        });
        dbContext.SaveChanges();
    }

    static UserConfig SeedUserConfig(ApplicationCommandDbContext dbContext, Guid userId)
    {
        var user = dbContext.Users.First(u => u.Id == userId);

        var config = new UserConfig
        {
            UserId = userId,
            IsDarkMode = false,
            IsHighContrast = false,
            EmailNotificationsEnabled = false,
            User = user
        };

        dbContext.UserConfigs.Add(config);
        dbContext.SaveChanges();
        return config;
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

    static async Task SeedUserSetReminderRowAsync(ApplicationCommandDbContext dbContext, Guid userId, int studyReminderId, int? hour)
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

    static async Task<Dictionary<int, int?>> GetUserSetReminderHoursAsync(ApplicationCommandDbContext dbContext, Guid userId)
    {
        var joinSet = dbContext.Set<UserSetStudyReminder>();

        var rows = await joinSet
            .Where(e => e.UserId == userId)
            .Select(e => new
            {
                e.StudyReminderId,
                e.Hour
            })
            .ToListAsync();

        return rows.ToDictionary(r => r.StudyReminderId, r => r.Hour);
    }

    static Mock<IValidator<UpdatePreferencesDto>> CreateValidatorMock(UpdatePreferencesDto dto)
    {
        var mock = new Mock<IValidator<UpdatePreferencesDto>>();
        mock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }
}
