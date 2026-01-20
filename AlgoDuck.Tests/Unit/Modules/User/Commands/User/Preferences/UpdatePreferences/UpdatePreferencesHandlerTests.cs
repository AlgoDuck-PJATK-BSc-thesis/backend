using System.Reflection;
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
    public async Task HandleAsync_WhenValid_ThenUpdatesUserConfig_AndSetsReminderColumns_AndComputesNextAt()
    {
        await using var dbContext = CreateCommandDbContext();

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

        Assert.True(GetBool(config, "IsDarkMode"));
        Assert.True(GetBool(config, "IsHighContrast"));
        Assert.True(GetBool(config, "EmailNotificationsEnabled"));

        Assert.Equal(9, GetNullableInt(config, "ReminderMonHour"));
        Assert.Null(GetNullableInt(config, "ReminderTueHour"));
        Assert.Equal(19, GetNullableInt(config, "ReminderWedHour"));
        Assert.Null(GetNullableInt(config, "ReminderThuHour"));
        Assert.Null(GetNullableInt(config, "ReminderFriHour"));
        Assert.Null(GetNullableInt(config, "ReminderSatHour"));
        Assert.Null(GetNullableInt(config, "ReminderSunHour"));

        var nextAt = GetNullableDateTimeOffset(config, "StudyReminderNextAtUtc");
        Assert.True(nextAt.HasValue);
        Assert.True(nextAt.Value > DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task HandleAsync_WhenEmailNotificationsDisabled_ThenClearsNextReminderAtUtc()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        SeedUser(dbContext, userId);
        var config = SeedUserConfig(dbContext, userId);

        SetNullableDateTimeOffset(config, "StudyReminderNextAtUtc", DateTimeOffset.UtcNow.AddHours(1));
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

        var nextAt = GetNullableDateTimeOffset(config, "StudyReminderNextAtUtc");
        Assert.Null(nextAt);
    }

    [Fact]
    public async Task HandleAsync_WhenRemindersEnabledButWeeklyRemindersEmpty_ThenClearsAllReminderColumns()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        SeedUser(dbContext, userId);
        var config = SeedUserConfig(dbContext, userId);

        SetNullableInt(config, "ReminderMonHour", 9);
        SetNullableInt(config, "ReminderWedHour", 19);
        SetNullableDateTimeOffset(config, "StudyReminderNextAtUtc", DateTimeOffset.UtcNow.AddHours(2));
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

        Assert.Null(GetNullableInt(config, "ReminderMonHour"));
        Assert.Null(GetNullableInt(config, "ReminderTueHour"));
        Assert.Null(GetNullableInt(config, "ReminderWedHour"));
        Assert.Null(GetNullableInt(config, "ReminderThuHour"));
        Assert.Null(GetNullableInt(config, "ReminderFriHour"));
        Assert.Null(GetNullableInt(config, "ReminderSatHour"));
        Assert.Null(GetNullableInt(config, "ReminderSunHour"));

        var nextAt = GetNullableDateTimeOffset(config, "StudyReminderNextAtUtc");
        Assert.True(nextAt.HasValue);
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

    static Mock<IValidator<UpdatePreferencesDto>> CreateValidatorMock(UpdatePreferencesDto dto)
    {
        var mock = new Mock<IValidator<UpdatePreferencesDto>>();
        mock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }

    static object SeedUserConfig(ApplicationCommandDbContext dbContext, Guid userId)
    {
        var config = CreateEntity("UserConfig");
        SetProperty(config, "UserId", userId);

        SetBool(config, "IsDarkMode", false);
        SetBool(config, "IsHighContrast", false);
        SetBool(config, "EmailNotificationsEnabled", false);

        SetNullableInt(config, "ReminderMonHour", null);
        SetNullableInt(config, "ReminderTueHour", null);
        SetNullableInt(config, "ReminderWedHour", null);
        SetNullableInt(config, "ReminderThuHour", null);
        SetNullableInt(config, "ReminderFriHour", null);
        SetNullableInt(config, "ReminderSatHour", null);
        SetNullableInt(config, "ReminderSunHour", null);

        SetNullableDateTimeOffset(config, "StudyReminderNextAtUtc", null);

        dbContext.Add(config);
        dbContext.SaveChanges();
        return config;
    }

    static object CreateEntity(string typeName)
    {
        var assembly = typeof(ApplicationUser).Assembly;
        var type = assembly.GetTypes().Single(t => t.Name == typeName);
        return Activator.CreateInstance(type)!;
    }

    static void SetProperty(object target, string propertyName, object value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        prop.SetValue(target, value);
    }

    static void SetBool(object target, string propertyName, bool value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        prop.SetValue(target, value);
    }

    static void SetNullableInt(object target, string propertyName, int? value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        prop.SetValue(target, value);
    }

    static void SetNullableDateTimeOffset(object target, string propertyName, DateTimeOffset? value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        prop.SetValue(target, value);
    }

    static bool GetBool(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        return (bool)(prop.GetValue(target) ?? false);
    }

    static int? GetNullableInt(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        return (int?)prop.GetValue(target);
    }

    static DateTimeOffset? GetNullableDateTimeOffset(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        return (DateTimeOffset?)prop.GetValue(target);
    }
}
