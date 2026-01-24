using AlgoDuck.Modules.User.Queries.User.Activity.GetUserAchievements;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Activity.GetUserAchievements;

public sealed class GetUserAchievementsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNoFilters_ThenReturnsPagedResults()
    {
        var userId = Guid.NewGuid();
        var baseCompletedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var achievements = Enumerable.Range(1, 10)
            .Select(i => new AchievementProgress
            {
                Code = $"ACH_{i:D2}",
                Name = $"Name_{i:D2}",
                Description = $"Desc_{i:D2}",
                CurrentValue = i,
                TargetValue = 10,
                IsCompleted = i % 2 == 0,
                CompletedAt = i % 2 == 0 ? baseCompletedAt.AddDays(i) : null
            })
            .ToList()
            .AsReadOnly();

        var achievementService = new Mock<IAchievementService>();
        achievementService.Setup(x => x.GetAchievementsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        var achievementSyncService = new Mock<IUserAchievementSyncService>();

        var handler = new GetUserAchievementsHandler(achievementService.Object, achievementSyncService.Object);

        var result = await handler.HandleAsync(userId, new GetUserAchievementsRequestDto
        {
            Page = 2,
            PageSize = 3
        }, CancellationToken.None);

        achievementSyncService.Verify(x => x.SyncAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(3, result.Count);
        Assert.Equal("ACH_04", result[0].Code);
        Assert.Equal("ACH_06", result[2].Code);

        Assert.True(result[0].IsCompleted);
        Assert.NotNull(result[0].CompletedAt);
        Assert.Equal(baseCompletedAt.AddDays(4), result[0].CompletedAt);

        Assert.False(result[1].IsCompleted);
        Assert.Null(result[1].CompletedAt);

        Assert.True(result[2].IsCompleted);
        Assert.NotNull(result[2].CompletedAt);
        Assert.Equal(baseCompletedAt.AddDays(6), result[2].CompletedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenCompletedFilterProvided_ThenFiltersByCompletion()
    {
        var userId = Guid.NewGuid();
        var completedAt1 = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var completedAt2 = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var achievements = new List<AchievementProgress>
        {
            new() { Code = "A1", Name = "N1", Description = "D1", CurrentValue = 1, TargetValue = 2, IsCompleted = false, CompletedAt = null },
            new() { Code = "A2", Name = "N2", Description = "D2", CurrentValue = 2, TargetValue = 2, IsCompleted = true, CompletedAt = completedAt1 },
            new() { Code = "A3", Name = "N3", Description = "D3", CurrentValue = 2, TargetValue = 2, IsCompleted = true, CompletedAt = completedAt2 }
        }.AsReadOnly();

        var achievementService = new Mock<IAchievementService>();
        achievementService.Setup(x => x.GetAchievementsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        var achievementSyncService = new Mock<IUserAchievementSyncService>();

        var handler = new GetUserAchievementsHandler(achievementService.Object, achievementSyncService.Object);

        var result = await handler.HandleAsync(userId, new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 20,
            Completed = true
        }, CancellationToken.None);

        achievementSyncService.Verify(x => x.SyncAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.True(x.IsCompleted));
        Assert.All(result, x => Assert.NotNull(x.CompletedAt));
        Assert.Contains(result, x => x.Code == "A2" && x.CompletedAt == completedAt1);
        Assert.Contains(result, x => x.Code == "A3" && x.CompletedAt == completedAt2);
    }

    [Fact]
    public async Task HandleAsync_WhenCodeFilterProvided_ThenFiltersByCodeCaseInsensitive()
    {
        var userId = Guid.NewGuid();
        var completedAt = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc);

        var achievements = new List<AchievementProgress>
        {
            new() { Code = "run_10", Name = "N1", Description = "D1", CurrentValue = 1, TargetValue = 2, IsCompleted = false, CompletedAt = completedAt },
            new() { Code = "RUN_20", Name = "N2", Description = "D2", CurrentValue = 2, TargetValue = 2, IsCompleted = true, CompletedAt = completedAt },
            new() { Code = "other", Name = "N3", Description = "D3", CurrentValue = 2, TargetValue = 2, IsCompleted = true, CompletedAt = completedAt }
        }.AsReadOnly();

        var achievementService = new Mock<IAchievementService>();
        achievementService.Setup(x => x.GetAchievementsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);

        var achievementSyncService = new Mock<IUserAchievementSyncService>();

        var handler = new GetUserAchievementsHandler(achievementService.Object, achievementSyncService.Object);

        var result = await handler.HandleAsync(userId, new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 20,
            CodeFilter = "RuN"
        }, CancellationToken.None);

        achievementSyncService.Verify(x => x.SyncAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Code == "run_10");
        Assert.Contains(result, x => x.Code == "RUN_20");

        var run10 = result.Single(x => x.Code == "run_10");
        Assert.False(run10.IsCompleted);
        Assert.Null(run10.CompletedAt);

        var run20 = result.Single(x => x.Code == "RUN_20");
        Assert.True(run20.IsCompleted);
        Assert.NotNull(run20.CompletedAt);
    }
}
