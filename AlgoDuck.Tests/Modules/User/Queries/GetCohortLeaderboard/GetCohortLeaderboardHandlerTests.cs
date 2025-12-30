using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.GetCohortLeaderboard;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AlgoDuck.Tests.Modules.User.Queries.GetCohortLeaderboard;

public sealed class GetCohortLeaderboardHandlerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GetCohortLeaderboardHandlerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task HandleAsync_WhenCohortIdEmpty_ThenThrowsValidationException()
    {
        await using var dbContext = CreateQueryDbContext();
        var handler = new GetCohortLeaderboardHandler(dbContext, new TestS3AvatarUrlGenerator());

        var dto = new GetCohortLeaderboardRequestDto
        {
            CohortId = Guid.Empty,
            Page = 1,
            PageSize = 20
        };

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenPageAndPageSizeInvalid_ThenUsesDefaults()
    {
        await using var dbContext = CreateQueryDbContext();

        var cohortId = Guid.NewGuid();
        SeedUsers(dbContext, cohortId, new[]
        {
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "u1", Experience = 10, AmountSolved = 1, CohortId = cohortId, Email = "u1@test.local", SecurityStamp = Guid.NewGuid().ToString() }
        });

        var handler = new GetCohortLeaderboardHandler(dbContext, new TestS3AvatarUrlGenerator());

        var dto = new GetCohortLeaderboardRequestDto
        {
            CohortId = cohortId,
            Page = 0,
            PageSize = 0
        };

        var result = await handler.HandleAsync(dto, CancellationToken.None);

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

        var cohortId = Guid.NewGuid();
        SeedUsers(dbContext, cohortId, new[]
        {
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "u1", Experience = 10, AmountSolved = 1, CohortId = cohortId, Email = "u1@test.local", SecurityStamp = Guid.NewGuid().ToString() }
        });

        var handler = new GetCohortLeaderboardHandler(dbContext, new TestS3AvatarUrlGenerator());

        var dto = new GetCohortLeaderboardRequestDto
        {
            CohortId = cohortId,
            Page = 1,
            PageSize = 500
        };

        var result = await handler.HandleAsync(dto, CancellationToken.None);

        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_WhenUsersInDifferentCohorts_ThenReturnsOnlyRequestedCohort()
    {
        await using var dbContext = CreateQueryDbContext();

        var cohortA = Guid.NewGuid();
        var cohortB = Guid.NewGuid();

        var users = new[]
        {
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "a1", Experience = 10, AmountSolved = 1, CohortId = cohortA, Email = "a1@test.local", SecurityStamp = Guid.NewGuid().ToString() },
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "a2", Experience = 20, AmountSolved = 1, CohortId = cohortA, Email = "a2@test.local", SecurityStamp = Guid.NewGuid().ToString() },
            new ApplicationUser { Id = Guid.NewGuid(), UserName = "b1", Experience = 999, AmountSolved = 999, CohortId = cohortB, Email = "b1@test.local", SecurityStamp = Guid.NewGuid().ToString() }
        };

        dbContext.ApplicationUsers.AddRange(users);
        await dbContext.SaveChangesAsync();

        var handler = new GetCohortLeaderboardHandler(dbContext, new TestS3AvatarUrlGenerator());

        var result = await handler.HandleAsync(new GetCohortLeaderboardRequestDto
        {
            CohortId = cohortA,
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Equal(2, result.TotalUsers);
        Assert.Equal(2, result.Entries.Count);
        Assert.All(result.Entries, e => Assert.Equal(cohortA, e.CohortId));
    }

    [Fact]
    public async Task HandleAsync_WhenOrderingByExperienceThenSolvedThenUsername_ThenRanksCorrectly()
    {
        await using var dbContext = CreateQueryDbContext();

        var cohortId = Guid.NewGuid();

        var u1 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "bbb", Experience = 100, AmountSolved = 5, CohortId = cohortId, Email = "u1@test.local", SecurityStamp = Guid.NewGuid().ToString() };
        var u2 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "aaa", Experience = 100, AmountSolved = 5, CohortId = cohortId, Email = "u2@test.local", SecurityStamp = Guid.NewGuid().ToString() };
        var u3 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "ccc", Experience = 100, AmountSolved = 4, CohortId = cohortId, Email = "u3@test.local", SecurityStamp = Guid.NewGuid().ToString() };
        var u4 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "ddd", Experience = 101, AmountSolved = 0, CohortId = cohortId, Email = "u4@test.local", SecurityStamp = Guid.NewGuid().ToString() };

        dbContext.ApplicationUsers.AddRange(u1, u2, u3, u4);
        await dbContext.SaveChangesAsync();

        var handler = new GetCohortLeaderboardHandler(dbContext, new TestS3AvatarUrlGenerator());

        var result = await handler.HandleAsync(new GetCohortLeaderboardRequestDto
        {
            CohortId = cohortId,
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Equal(4, result.Entries.Count);

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

        var cohortId = Guid.NewGuid();

        var users = Enumerable.Range(1, 30)
            .Select(i => new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = $"u{i:D2}",
                Experience = 1000 - i,
                AmountSolved = i,
                CohortId = cohortId,
                Email = $"u{i:D2}@test.local",
                SecurityStamp = Guid.NewGuid().ToString()
            })
            .ToList();

        dbContext.ApplicationUsers.AddRange(users);
        await dbContext.SaveChangesAsync();

        var handler = new GetCohortLeaderboardHandler(dbContext, new TestS3AvatarUrlGenerator());

        var result = await handler.HandleAsync(new GetCohortLeaderboardRequestDto
        {
            CohortId = cohortId,
            Page = 2,
            PageSize = 10
        }, CancellationToken.None);

        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(30, result.TotalUsers);
        Assert.Equal(10, result.Entries.Count);

        Assert.Equal(11, result.Entries[0].Rank);
        Assert.Equal(20, result.Entries[9].Rank);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasSelectedPurchase_ThenReturnsAvatarUrl()
    {
        await using var dbContext = CreateQueryDbContext();

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var duckItemId = Guid.NewGuid();
        var rarityId = Guid.NewGuid();

        dbContext.Rarities.Add(new Rarity
        {
            RarityId = rarityId,
            RarityName = "test"
        });

        dbContext.Items.Add(new DuckItem
        {
            ItemId = duckItemId,
            Name = "duck",
            Description = "duck",
            Price = 0,
            Purchasable = true,
            RarityId = rarityId
        });

        dbContext.ApplicationUsers.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "u1",
            Experience = 10,
            AmountSolved = 1,
            CohortId = cohortId,
            Email = "u1@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        });

        dbContext.Purchases.Add(new DuckOwnership
        {
            UserId = userId,
            ItemId = duckItemId,
            SelectedAsAvatar = true
        });

        await dbContext.SaveChangesAsync();

        var handler = new GetCohortLeaderboardHandler(dbContext, new TestS3AvatarUrlGenerator());

        var result = await handler.HandleAsync(new GetCohortLeaderboardRequestDto
        {
            CohortId = cohortId,
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Single(result.Entries);
        Assert.False(string.IsNullOrWhiteSpace(result.Entries[0].UserAvatarUrl));
    }

    static ApplicationQueryDbContext CreateQueryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    static void SeedUsers(ApplicationQueryDbContext dbContext, Guid cohortId, IEnumerable<ApplicationUser> users)
    {
        var list = users.ToList();

        foreach (var u in list)
        {
            u.CohortId = cohortId;
            if (string.IsNullOrWhiteSpace(u.Email))
            {
                u.Email = $"{u.UserName ?? "user"}@test.local";
            }

            if (string.IsNullOrWhiteSpace(u.SecurityStamp))
            {
                u.SecurityStamp = Guid.NewGuid().ToString();
            }
        }

        dbContext.ApplicationUsers.AddRange(list);
        dbContext.SaveChanges();
    }

    private sealed class TestS3AvatarUrlGenerator : IS3AvatarUrlGenerator
    {
        public string GetAvatarUrl(string avatarKey)
        {
            return string.IsNullOrWhiteSpace(avatarKey) ? string.Empty : $"https://test.local/{avatarKey}";
        }
    }
}