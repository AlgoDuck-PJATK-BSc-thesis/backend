using AlgoDuck.Modules.User.Queries.GetUserAchievements;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserAchievements;

public sealed class GetUserAchievementsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNoFilters_ThenReturnsPagedResults()
    {
        var userId = Guid.NewGuid();

        var achievements = Enumerable.Range(1, 10)
            .Select(i => new AchievementProgress
            {
                Code = $"ACH_{i:D2}",
                Name = $"Name_{i:D2}",
                Description = $"Desc_{i:D2}",
                CurrentValue = i,
                TargetValue = 10,
                IsCompleted = i % 2 == 0
            })
            .ToList()
            .AsReadOnly();

        var achievementService = new Mock<IAchievementService>();
        achievementService.Setup(x => x.GetAchievementsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        var handler = new GetUserAchievementsHandler(achievementService.Object);

        var result = await handler.HandleAsync(userId, new GetUserAchievementsRequestDto
        {
            Page = 2,
            PageSize = 3
        }, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("ACH_04", result[0].Code);
        Assert.Equal("ACH_06", result[2].Code);
    }

    [Fact]
    public async Task HandleAsync_WhenCompletedFilterProvided_ThenFiltersByCompletion()
    {
        var userId = Guid.NewGuid();

        var achievements = new List<AchievementProgress>
        {
            new() { Code = "A1", Name = "N1", Description = "D1", CurrentValue = 1, TargetValue = 2, IsCompleted = false },
            new() { Code = "A2", Name = "N2", Description = "D2", CurrentValue = 2, TargetValue = 2, IsCompleted = true },
            new() { Code = "A3", Name = "N3", Description = "D3", CurrentValue = 2, TargetValue = 2, IsCompleted = true }
        }.AsReadOnly();

        var achievementService = new Mock<IAchievementService>();
        achievementService.Setup(x => x.GetAchievementsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        var handler = new GetUserAchievementsHandler(achievementService.Object);

        var result = await handler.HandleAsync(userId, new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 20,
            Completed = true
        }, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.True(x.IsCompleted));
    }

    [Fact]
    public async Task HandleAsync_WhenCodeFilterProvided_ThenFiltersByCodeCaseInsensitive()
    {
        var userId = Guid.NewGuid();

        var achievements = new List<AchievementProgress>
        {
            new() { Code = "run_10", Name = "N1", Description = "D1", CurrentValue = 1, TargetValue = 2, IsCompleted = false },
            new() { Code = "RUN_20", Name = "N2", Description = "D2", CurrentValue = 2, TargetValue = 2, IsCompleted = true },
            new() { Code = "other", Name = "N3", Description = "D3", CurrentValue = 2, TargetValue = 2, IsCompleted = true }
        }.AsReadOnly();

        var achievementService = new Mock<IAchievementService>();
        achievementService.Setup(x => x.GetAchievementsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        var handler = new GetUserAchievementsHandler(achievementService.Object);

        var result = await handler.HandleAsync(userId, new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 20,
            CodeFilter = "RuN"
        }, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Code == "run_10");
        Assert.Contains(result, x => x.Code == "RUN_20");
    }
}