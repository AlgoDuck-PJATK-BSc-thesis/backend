using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorTheme;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Commands.SetEditorTheme;

public sealed class SetEditorThemeHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        await using var dbContext = CreateCommandDbContext();

        var dto = new SetEditorThemeDto { EditorThemeId = Guid.NewGuid() };
        var validator = CreateValidatorMock(dto);

        var handler = new SetEditorThemeHandler(dbContext, validator.Object);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserConfigMissing_ThenThrowsUserNotFoundException()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        var dto = new SetEditorThemeDto { EditorThemeId = Guid.NewGuid() };
        var validator = CreateValidatorMock(dto);

        var handler = new SetEditorThemeHandler(dbContext, validator.Object);

        var ex = await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("configuration", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenThemeMissing_ThenThrowsValidationException()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        SeedUserWithConfig(dbContext, userId);

        var dto = new SetEditorThemeDto { EditorThemeId = Guid.NewGuid() };
        var validator = CreateValidatorMock(dto);

        var handler = new SetEditorThemeHandler(dbContext, validator.Object);

        var ex = await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("theme", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenLayoutMissing_ThenCreatesLayoutWithTheme()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        SeedUserWithConfig(dbContext, userId);

        var themeId = Guid.NewGuid();
        SeedEditorTheme(dbContext, themeId);

        var dto = new SetEditorThemeDto { EditorThemeId = themeId };
        var validator = CreateValidatorMock(dto);

        var handler = new SetEditorThemeHandler(dbContext, validator.Object);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        var layout = await dbContext.EditorLayouts.SingleAsync();
        Assert.Equal(userId, layout.UserConfigId);
        Assert.Equal(themeId, layout.EditorThemeId);
    }

    [Fact]
    public async Task HandleAsync_WhenLayoutExists_ThenUpdatesTheme()
    {
        await using var dbContext = CreateCommandDbContext();

        var userId = Guid.NewGuid();
        var config = SeedUserWithConfig(dbContext, userId);

        var theme1 = Guid.NewGuid();
        var theme2 = Guid.NewGuid();
        var t1 = SeedEditorTheme(dbContext, theme1);
        var t2 = SeedEditorTheme(dbContext, theme2);

        SeedEditorLayout(dbContext, config, t1);

        var dto = new SetEditorThemeDto { EditorThemeId = theme2 };
        var validator = CreateValidatorMock(dto);

        var handler = new SetEditorThemeHandler(dbContext, validator.Object);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        var layout = await dbContext.EditorLayouts.SingleAsync();
        Assert.Equal(userId, layout.UserConfigId);
        Assert.Equal(theme2, layout.EditorThemeId);
    }

    static ApplicationCommandDbContext CreateCommandDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    static Mock<IValidator<SetEditorThemeDto>> CreateValidatorMock(SetEditorThemeDto dto)
    {
        var mock = new Mock<IValidator<SetEditorThemeDto>>();
        mock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }

    static UserConfig SeedUserWithConfig(ApplicationCommandDbContext dbContext, Guid userId)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"user_{userId:N}",
            Email = $"user_{userId:N}@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var config = new UserConfig
        {
            UserId = userId,
            User = user,
            IsDarkMode = false,
            IsHighContrast = false,
            Language = "en",
            EmailNotificationsEnabled = false,
            PushNotificationsEnabled = false
        };

        dbContext.ApplicationUsers.Add(user);
        dbContext.UserConfigs.Add(config);
        dbContext.SaveChanges();

        return config;
    }

    static EditorTheme SeedEditorTheme(ApplicationCommandDbContext dbContext, Guid themeId)
    {
        var theme = new EditorTheme
        {
            EditorThemeId = themeId,
            ThemeName = $"Theme_{themeId:N}"
        };

        dbContext.EditorThemes.Add(theme);
        dbContext.SaveChanges();

        return theme;
    }

    static EditorLayout SeedEditorLayout(ApplicationCommandDbContext dbContext, UserConfig config, EditorTheme theme)
    {
        var layout = new EditorLayout
        {
            EditorLayoutId = Guid.NewGuid(),
            UserConfigId = config.UserId,
            EditorThemeId = theme.EditorThemeId,
            LayoutName = "",
            UserConfig = config,
            EditorTheme = theme
        };

        dbContext.EditorLayouts.Add(layout);
        dbContext.SaveChanges();

        return layout;
    }
}