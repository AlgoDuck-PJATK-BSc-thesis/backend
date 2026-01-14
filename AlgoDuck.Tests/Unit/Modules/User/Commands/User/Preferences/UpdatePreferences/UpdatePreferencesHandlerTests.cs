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
    public async Task HandleAsync_WhenUserConfigMissing_ThenThrowsUserNotFoundException()
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

        var ex = await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("configuration", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenUpdatesUserConfig_AndSetsReminderColumns_AndComputesNextAt()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
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
        var config = SeedUserConfig(dbContext, userId);

        SetNullableDateTimeOffsetIfPresent(config, "StudyReminderNextAtUtc", DateTimeOffset.UtcNow.AddHours(1));
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

    static ApplicationCommandDbContext CreateCommandDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
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
        SetGuidIfPresent(config, "UserConfigId", userId);
        SetProperty(config, "UserId", userId);

        SetBoolIfPresent(config, "IsDarkMode", false);
        SetBoolIfPresent(config, "IsHighContrast", false);
        SetBoolIfPresent(config, "EmailNotificationsEnabled", false);

        SetNullableIntIfPresent(config, "ReminderMonHour", null);
        SetNullableIntIfPresent(config, "ReminderTueHour", null);
        SetNullableIntIfPresent(config, "ReminderWedHour", null);
        SetNullableIntIfPresent(config, "ReminderThuHour", null);
        SetNullableIntIfPresent(config, "ReminderFriHour", null);
        SetNullableIntIfPresent(config, "ReminderSatHour", null);
        SetNullableIntIfPresent(config, "ReminderSunHour", null);

        SetNullableDateTimeOffsetIfPresent(config, "StudyReminderNextAtUtc", null);

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

    static void SetGuidIfPresent(object target, string propertyName, Guid value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return;

        if (prop.PropertyType == typeof(Guid))
        {
            prop.SetValue(target, value);
            return;
        }

        if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(Guid))
        {
            prop.SetValue(target, value);
        }
    }

    static void SetBoolIfPresent(object target, string propertyName, bool value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return;

        if (prop.PropertyType == typeof(bool))
        {
            prop.SetValue(target, value);
            return;
        }

        if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(bool))
        {
            prop.SetValue(target, value);
        }
    }

    static void SetNullableIntIfPresent(object target, string propertyName, int? value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return;

        if (prop.PropertyType == typeof(int?))
        {
            prop.SetValue(target, value);
        }
    }

    static void SetNullableDateTimeOffsetIfPresent(object target, string propertyName, DateTimeOffset? value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return;

        if (prop.PropertyType == typeof(DateTimeOffset?))
        {
            prop.SetValue(target, value);
        }
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
