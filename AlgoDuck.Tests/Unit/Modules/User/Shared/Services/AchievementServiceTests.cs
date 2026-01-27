using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Services;

public sealed class AchievementServiceTests
{
    private static (ApplicationQueryDbContext queryContext, ApplicationCommandDbContext commandContext) CreateContexts()
    {
        var dbName = Guid.NewGuid().ToString();
        var queryOptions = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var commandOptions = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return (new ApplicationQueryDbContext(queryOptions), new ApplicationCommandDbContext(commandOptions));
    }

    [Fact]
    public async Task GetAchievementsAsync_WhenNoAchievements_ReturnsEmptyList()
    {
        var (queryContext, _) = CreateContexts();
        var service = new AchievementService(queryContext, null!);
        var userId = Guid.NewGuid();

        var result = await service.GetAchievementsAsync(userId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AwardAchievement_NewAchievement_IsAdded()
    {
        var (queryContext, commandContext) = CreateContexts();
        var service = new AchievementService(queryContext, commandContext);
        var userId = Guid.NewGuid();
        var achievementCode = "TEST_001";

        await commandContext.Achievements.AddAsync(new Achievement
        {
            Code = achievementCode,
            Name = "Test Achievement",
            Description = "A test achievement",
            TargetValue = 1,
            CreatedAt = DateTime.UtcNow
        });
        await commandContext.SaveChangesAsync();

        await service.AwardAchievement(userId, achievementCode);

        var awarded = await queryContext.UserAchievements
            .SingleOrDefaultAsync(a => a.UserId == userId && a.AchievementCode == achievementCode);

        awarded.Should().NotBeNull();
        awarded.IsCompleted.Should().BeTrue();
        awarded.CurrentValue.Should().Be(1);
        awarded.AchievementCode.Should().Be(achievementCode);
    }

    [Fact]
    public async Task AwardAchievement_ExistingAchievement_NotDuplicated()
    {
        var (queryContext, commandContext) = CreateContexts();
        var service = new AchievementService(queryContext, commandContext);
        var userId = Guid.NewGuid();
        var achievementCode = "TEST_001";

        await commandContext.Achievements.AddAsync(new Achievement
        {
            Code = achievementCode,
            Name = "Test Achievement",
            Description = "A test achievement",
            TargetValue = 1,
            CreatedAt = DateTime.UtcNow
        });
        await commandContext.SaveChangesAsync();

        await commandContext.UserAchievements.AddAsync(new UserAchievement
        {
            UserId = userId,
            AchievementCode = achievementCode,
            IsCompleted = true,
            CurrentValue = 1
        });
        await commandContext.SaveChangesAsync();

        var countBefore = await queryContext.UserAchievements.CountAsync(a => a.UserId == userId);

        await service.AwardAchievement(userId, achievementCode);

        var countAfter = await queryContext.UserAchievements.CountAsync(a => a.UserId == userId);
        countAfter.Should().Be(countBefore);
    }

    [Fact]
    public async Task AwardAchievement_AchievementNotInCatalog_ThrowsException()
    {
        var (queryContext, commandContext) = CreateContexts();
        var service = new AchievementService(queryContext, commandContext);
        var userId = Guid.NewGuid();
        var achievementCode = "NONEXISTENT";

        var act = async () => await service.AwardAchievement(userId, achievementCode);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Achievement with code '{achievementCode}' not found in catalog.");
    }

    [Fact]
    public async Task GetAchievementsAsync_WithAchievements_ReturnsCorrectData()
    {
        var (queryContext, commandContext) = CreateContexts();
        var service = new AchievementService(queryContext, commandContext);
        var userId = Guid.NewGuid();
        var achievementCode = "TEST_001";

        await commandContext.Achievements.AddAsync(new Achievement
        {
            Code = achievementCode,
            Name = "Test Achievement",
            Description = "A test achievement",
            TargetValue = 10,
            CreatedAt = DateTime.UtcNow
        });

        await commandContext.UserAchievements.AddAsync(new UserAchievement
        {
            UserId = userId,
            AchievementCode = achievementCode,
            CurrentValue = 5,
            IsCompleted = false
        });
        await commandContext.SaveChangesAsync();

        var result = await service.GetAchievementsAsync(userId, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Code.Should().Be(achievementCode);
        result[0].Name.Should().Be("Test Achievement");
        result[0].Description.Should().Be("A test achievement");
        result[0].CurrentValue.Should().Be(5);
        result[0].TargetValue.Should().Be(10);
        result[0].IsCompleted.Should().BeFalse();
    }
}
