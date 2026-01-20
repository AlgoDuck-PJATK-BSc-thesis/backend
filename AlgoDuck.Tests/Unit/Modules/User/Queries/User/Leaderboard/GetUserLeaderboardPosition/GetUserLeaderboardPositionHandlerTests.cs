using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserLeaderboardPosition;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Leaderboard.GetUserLeaderboardPosition;

public sealed class GetUserLeaderboardPositionHandlerTests
{
    private const string AdminRoleName = "admin";

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        await using var dbContext = CreateQueryDbContext();
        await SeedAdminRoleAsync(dbContext);

        var handler = new GetUserLeaderboardPositionHandler(dbContext);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAdmin_ThenThrowsUserNotFoundException()
    {
        await using var dbContext = CreateQueryDbContext();

        var adminRoleId = await SeedAdminRoleAsync(dbContext);
        var adminId = Guid.NewGuid();

        var admin = new ApplicationUser
        {
            Id = adminId,
            UserName = "admin",
            Email = "admin@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 99999,
            AmountSolved = 999
        };

        await SeedAdminUserAsync(dbContext, adminRoleId, admin);

        var handler = new GetUserLeaderboardPositionHandler(dbContext);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(adminId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ThenReturnsRankAndPercentile()
    {
        await using var dbContext = CreateQueryDbContext();

        var adminRoleId = await SeedAdminRoleAsync(dbContext);
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 99999,
            AmountSolved = 999
        };
        await SeedAdminUserAsync(dbContext, adminRoleId, admin);

        var targetId = Guid.NewGuid();

        var u1 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "u1",
            Email = "u1@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 200,
            AmountSolved = 1
        };

        var u2 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "u2",
            Email = "u2@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 100,
            AmountSolved = 6
        };

        var target = new ApplicationUser
        {
            Id = targetId,
            UserName = "target",
            Email = "target@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 100,
            AmountSolved = 5
        };

        var u3 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "u3",
            Email = "u3@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 90,
            AmountSolved = 999
        };

        dbContext.ApplicationUsers.AddRange(u1, u2, target, u3);
        await dbContext.SaveChangesAsync();

        var handler = new GetUserLeaderboardPositionHandler(dbContext);

        var result = await handler.HandleAsync(targetId, CancellationToken.None);

        Assert.Equal(targetId, result.UserId);
        Assert.Equal(3, result.Rank);
        Assert.Equal(4, result.TotalUsers);
        Assert.Equal(100, result.Experience);
        Assert.Equal(5, result.AmountSolved);
        Assert.Equal(25.0, result.Percentile, 10);
    }

    private static async Task<Guid> SeedAdminRoleAsync(ApplicationQueryDbContext dbContext)
    {
        var roleId = Guid.NewGuid();

        dbContext.Set<IdentityRole<Guid>>().Add(new IdentityRole<Guid>
        {
            Id = roleId,
            Name = AdminRoleName,
            NormalizedName = AdminRoleName.ToUpperInvariant()
        });

        await dbContext.SaveChangesAsync();
        return roleId;
    }

    private static async Task SeedAdminUserAsync(ApplicationQueryDbContext dbContext, Guid adminRoleId, ApplicationUser adminUser)
    {
        dbContext.ApplicationUsers.Add(adminUser);
        dbContext.Set<IdentityUserRole<Guid>>().Add(new IdentityUserRole<Guid>
        {
            RoleId = adminRoleId,
            UserId = adminUser.Id
        });

        await dbContext.SaveChangesAsync();
    }

    static ApplicationQueryDbContext CreateQueryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }
}
