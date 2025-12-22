using System.Threading;
using System.Threading.Tasks;
using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Modules.User.Shared.Services;

public sealed class AchievementServiceTests
{
    private static ApplicationQueryDbContext CreateQueryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    [Fact]
    public async Task GetAchievementsAsync_WhenNoAchievements_ReturnsEmptyList()
    {
        using var context = CreateQueryContext();
        var service = new AchievementService(context);
        var userId = Guid.NewGuid();

        var result = await service.GetAchievementsAsync(userId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}