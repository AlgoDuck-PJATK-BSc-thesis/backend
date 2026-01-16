using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Leaderboard.GetLeaderboardGlobal;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Leaderboard.GetLeaderboardGlobal;

public sealed class GetLeaderboardGlobalHandlerTests
{
    private const string AdminRoleName = "admin";

    [Fact]
    public async Task HandleAsync_WhenPageAndPageSizeInvalid_ThenUsesDefaults()
    {
        await using var dbContext = CreateQueryDbContext();
        await SeedAdminRoleAsync(dbContext);

        dbContext.ApplicationUsers.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "u1",
            Experience = 10,
            AmountSolved = 1,
            Email = "u1@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        });

        await dbContext.SaveChangesAsync();

        var handler = new GetLeaderboardGlobalHandler(dbContext);

        var result = await handler.HandleAsync(new GetLeaderboardGlobalRequestDto
        {
            Page = 0,
            PageSize = 0
        }, CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(1, result.TotalUsers);
        Assert.Single(result.Entries);
        Assert.Equal(1, result.Entries[0].Rank);
    }

    [Fact]
    public async Task HandleAsync_WhenPageSizeTooLarge_ThenCapsTo100()
    {
        await using var dbContext = CreateQueryDbContext();
        await SeedAdminRoleAsync(dbContext);

        dbContext.ApplicationUsers.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "u1",
            Experience = 10,
            AmountSolved = 1,
            Email = "u1@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        });

        await dbContext.SaveChangesAsync();

        var handler = new GetLeaderboardGlobalHandler(dbContext);

        var result = await handler.HandleAsync(new GetLeaderboardGlobalRequestDto
        {
            Page = 1,
            PageSize = 1000
        }, CancellationToken.None);

        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderingByExperienceThenSolvedThenUsername_ThenRanksCorrectly()
    {
        await using var dbContext = CreateQueryDbContext();

        var adminRoleId = await SeedAdminRoleAsync(dbContext);
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Experience = 99999,
            AmountSolved = 999,
            Email = "admin@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await SeedAdminUserAsync(dbContext, adminRoleId, admin);

        var u1 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "bbb", Experience = 100, AmountSolved = 5, Email = "u1@test.local", SecurityStamp = Guid.NewGuid().ToString() };
        var u2 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "aaa", Experience = 100, AmountSolved = 5, Email = "u2@test.local", SecurityStamp = Guid.NewGuid().ToString() };
        var u3 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "ccc", Experience = 100, AmountSolved = 4, Email = "u3@test.local", SecurityStamp = Guid.NewGuid().ToString() };
        var u4 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "ddd", Experience = 101, AmountSolved = 0, Email = "u4@test.local", SecurityStamp = Guid.NewGuid().ToString() };

        dbContext.ApplicationUsers.AddRange(u1, u2, u3, u4);
        await dbContext.SaveChangesAsync();

        var handler = new GetLeaderboardGlobalHandler(dbContext);

        var result = await handler.HandleAsync(new GetLeaderboardGlobalRequestDto
        {
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Equal(4, result.TotalUsers);
        Assert.Equal(4, result.Entries.Count);
        Assert.DoesNotContain(result.Entries, e => e.UserId == admin.Id);

        Assert.Equal(u4.Id, result.Entries[0].UserId);
        Assert.Equal(1, result.Entries[0].Rank);

        Assert.Equal(u2.Id, result.Entries[1].UserId);
        Assert.Equal(2, result.Entries[1].Rank);

        Assert.Equal(u1.Id, result.Entries[2].UserId);
        Assert.Equal(3, result.Entries[2].Rank);

        Assert.Equal(u3.Id, result.Entries[3].UserId);
        Assert.Equal(4, result.Entries[3].Rank);
    }

    [Fact]
    public async Task HandleAsync_WhenPaging_ThenReturnsCorrectRanksAndEntries()
    {
        await using var dbContext = CreateQueryDbContext();

        var adminRoleId = await SeedAdminRoleAsync(dbContext);
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Experience = 99999,
            AmountSolved = 999,
            Email = "admin@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await SeedAdminUserAsync(dbContext, adminRoleId, admin);

        var users = Enumerable.Range(1, 30)
            .Select(i => new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = $"u{i:D2}",
                Experience = 1000 - i,
                AmountSolved = i,
                Email = $"u{i:D2}@test.local",
                SecurityStamp = Guid.NewGuid().ToString()
            })
            .ToList();

        dbContext.ApplicationUsers.AddRange(users);
        await dbContext.SaveChangesAsync();

        var handler = new GetLeaderboardGlobalHandler(dbContext);

        var result = await handler.HandleAsync(new GetLeaderboardGlobalRequestDto
        {
            Page = 2,
            PageSize = 10
        }, CancellationToken.None);

        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(30, result.TotalUsers);
        Assert.Equal(10, result.Entries.Count);
        Assert.DoesNotContain(result.Entries, e => e.UserId == admin.Id);

        Assert.Equal(11, result.Entries[0].Rank);
        Assert.Equal(20, result.Entries[9].Rank);
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
