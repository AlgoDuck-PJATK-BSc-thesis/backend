using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.UpdatePreferences;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Reflection;

namespace AlgoDuck.Tests.Modules.User.Commands.UpdatePreferences;

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
            Language = "en",
            EmailNotificationsEnabled = true,
            PushNotificationsEnabled = true
        };

        var validator = CreateValidatorMock(dto);
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object);

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
            Language = "en",
            EmailNotificationsEnabled = true,
            PushNotificationsEnabled = true
        };

        var validator = CreateValidatorMock(dto);
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object);

        var ex = await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("configuration", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenUpdatesUserConfig()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        var config = SeedUserConfig(dbContext, userId);

        var dto = new UpdatePreferencesDto
        {
            IsDarkMode = true,
            IsHighContrast = true,
            Language = "pl",
            EmailNotificationsEnabled = true,
            PushNotificationsEnabled = false
        };

        var validator = CreateValidatorMock(dto);
        var handler = new UpdatePreferencesHandler(dbContext, validator.Object);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(true, GetBool(config, "IsDarkMode"));
        Assert.Equal(true, GetBool(config, "IsHighContrast"));
        Assert.Equal("pl", GetString(config, "Language"));
        Assert.Equal(true, GetBool(config, "EmailNotificationsEnabled"));
        Assert.Equal(false, GetBool(config, "PushNotificationsEnabled"));
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
        SetStringIfPresent(config, "Language", "en");
        SetBoolIfPresent(config, "EmailNotificationsEnabled", false);
        SetBoolIfPresent(config, "PushNotificationsEnabled", false);

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
        prop!.SetValue(target, value);
    }

    static void SetGuidIfPresent(object target, string propertyName, Guid value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null)
        {
            return;
        }

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
        if (prop is null)
        {
            return;
        }

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

    static void SetStringIfPresent(object target, string propertyName, string value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null)
        {
            return;
        }

        if (prop.PropertyType == typeof(string))
        {
            prop.SetValue(target, value);
        }
    }

    static bool GetBool(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        return (bool)(prop!.GetValue(target) ?? false);
    }

    static string GetString(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        return (string)(prop!.GetValue(target) ?? "");
    }
}