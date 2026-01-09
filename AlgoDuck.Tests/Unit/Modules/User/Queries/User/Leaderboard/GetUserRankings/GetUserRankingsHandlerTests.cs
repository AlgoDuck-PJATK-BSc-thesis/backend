using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserRankings;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Leaderboard.GetUserRankings;

public sealed class GetUserRankingsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenPaging_ThenReturnsCorrectRanks()
    {
        await using var dbContext = CreateQueryDbContext();

        var users = Enumerable.Range(1, 30)
            .Select(i => new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = $"u{i:D2}",
                Email = $"u{i:D2}@test.local",
                SecurityStamp = Guid.NewGuid().ToString(),
                Experience = 1000 - i
            })
            .ToList();

        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync();

        var handler = new GetUserRankingsHandler(dbContext);

        var result = await handler.HandleAsync(new GetUserRankingsQuery
        {
            Page = 2,
            PageSize = 10
        }, CancellationToken.None);

        Assert.Equal(10, result.Count);
        Assert.Equal(11, result[0].Rank);
        Assert.Equal(20, result[9].Rank);
    }

    [Fact]
    public async Task HandleAsync_WhenOrdering_ThenOrdersByExperienceDescThenUsername()
    {
        await using var dbContext = CreateQueryDbContext();

        var a = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "aaa",
            Email = "aaa@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 100
        };

        var b = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "bbb",
            Email = "bbb@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 100
        };

        var top = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "zzz",
            Email = "zzz@test.local",
            SecurityStamp = Guid.NewGuid().ToString(),
            Experience = 101
        };

        dbContext.Users.AddRange(a, b, top);
        await dbContext.SaveChangesAsync();

        var handler = new GetUserRankingsHandler(dbContext);

        var result = await handler.HandleAsync(new GetUserRankingsQuery
        {
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(top.Id, result[0].UserId);
        Assert.Equal(1, result[0].Rank);

        Assert.Equal(a.Id, result[1].UserId);
        Assert.Equal(2, result[1].Rank);

        Assert.Equal(b.Id, result[2].UserId);
        Assert.Equal(3, result[2].Rank);
    }

    static ApplicationQueryDbContext CreateQueryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }
}