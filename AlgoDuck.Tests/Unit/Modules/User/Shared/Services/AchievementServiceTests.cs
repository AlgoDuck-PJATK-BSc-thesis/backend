using AlgoDuck.DAL;
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
        var achievementCode = "TestAchievement";

        await service.AwardAchievement(userId, achievementCode);

        var awarded = await queryContext.UserAchievements
            .SingleOrDefaultAsync(a => a.UserId == userId && a.Code == achievementCode);

        awarded.Should().NotBeNull();
        awarded.IsCompleted.Should().BeTrue();
        awarded.Name.Should().Contain("Testachievement");
        awarded.CurrentValue.Should().Be(1);
        awarded.TargetValue.Should().Be(1);
    }

    [Fact]
    public async Task AwardAchievement_ExistingAchievement_NotDuplicated()
    {
        var (queryContext, commandContext) = CreateContexts();
        var service = new AchievementService(queryContext, commandContext);
        var userId = Guid.NewGuid();
        var achievementCode = "TestAchievement";

        await commandContext.UserAchievements.AddAsync(new Models.UserAchievement
        {
            UserId = userId,
            Code = achievementCode,
            Name = "Existing",
            IsCompleted = true,
            CurrentValue = 1,
            TargetValue = 1
        });
        await commandContext.SaveChangesAsync();

        var countBefore = await queryContext.UserAchievements.CountAsync(a => a.UserId == userId);

        await service.AwardAchievement(userId, achievementCode);

        var countAfter = await queryContext.UserAchievements.CountAsync(a => a.UserId == userId);
        countAfter.Should().Be(countBefore);
    }
}
