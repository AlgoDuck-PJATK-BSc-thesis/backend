using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Queries.Identity.SearchUsersByEmail;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Queries.Identity.SearchUsersByEmail;

public sealed class SearchUsersByEmailHandlerTests
{
    static ApplicationCommandDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenQueryWhitespace_ReturnsEmpty()
    {
        await using var db = CreateDb();
        var handler = new SearchUsersByEmailHandler(db);

        var result = await handler.HandleAsync(new SearchUsersByEmailDto { Query = "   ", Limit = 20 }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_FiltersCaseInsensitively_OrdersAndRespectsLimit()
    {
        await using var db = CreateDb();

        var u1 = new ApplicationUser { Id = Guid.NewGuid(), Email = "b@example.com", UserName = "b", EmailConfirmed = true };
        var u2 = new ApplicationUser { Id = Guid.NewGuid(), Email = "A@Example.com", UserName = "a", EmailConfirmed = false };
        var u3 = new ApplicationUser { Id = Guid.NewGuid(), Email = "other@domain.com", UserName = "o", EmailConfirmed = true };

        db.ApplicationUsers.AddRange(u1, u2, u3);
        await db.SaveChangesAsync();

        var handler = new SearchUsersByEmailHandler(db);

        var result = await handler.HandleAsync(new SearchUsersByEmailDto { Query = "example", Limit = 1 }, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(u2.Id, result[0].Id);
        Assert.Equal(u2.Email, result[0].Email);
        Assert.Equal(u2.UserName, result[0].UserName);
        Assert.Equal(u2.EmailConfirmed, result[0].EmailConfirmed);
    }

    [Fact]
    public async Task HandleAsync_WhenLimitInvalid_UsesDefault20()
    {
        await using var db = CreateDb();
        var handler = new SearchUsersByEmailHandler(db);

        for (var i = 0; i < 25; i++)
        {
            db.ApplicationUsers.Add(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = $"user{i}@example.com",
                UserName = $"u{i}",
                EmailConfirmed = false
            });
        }

        await db.SaveChangesAsync();

        var result = await handler.HandleAsync(new SearchUsersByEmailDto { Query = "example", Limit = 0 }, CancellationToken.None);

        Assert.Equal(20, result.Count);
    }

    [Fact]
    public async Task HandleAsync_WhenLimitTooHigh_ClampsTo100()
    {
        await using var db = CreateDb();
        var handler = new SearchUsersByEmailHandler(db);

        for (var i = 0; i < 150; i++)
        {
            db.ApplicationUsers.Add(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = $"user{i}@example.com",
                UserName = $"u{i}",
                EmailConfirmed = false
            });
        }

        await db.SaveChangesAsync();

        var result = await handler.HandleAsync(new SearchUsersByEmailDto { Query = "example", Limit = 1000 }, CancellationToken.None);

        Assert.Equal(100, result.Count);
    }
}
