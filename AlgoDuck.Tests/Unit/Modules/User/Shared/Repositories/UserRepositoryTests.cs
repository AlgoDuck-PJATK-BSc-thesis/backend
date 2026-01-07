using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Repositories;

public sealed class UserRepositoryTests
{
    private static (UserRepository Repository, ApplicationQueryDbContext QueryContext, ApplicationCommandDbContext CommandContext) CreateRepository()
    {
        var dbName = Guid.NewGuid().ToString("N");

        var queryOptions = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var commandOptions = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var queryContext = new ApplicationQueryDbContext(queryOptions);
        var commandContext = new ApplicationCommandDbContext(commandOptions);

        var repository = new UserRepository(queryContext, commandContext);

        return (repository, queryContext, commandContext);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsUserWithConfig()
    {
        var (repository, queryContext, _) = CreateRepository();
        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "test-user",
            Email = "test@example.com",
            UserConfig = new UserConfig
            {
                UserId = userId,
                Language = "en"
            }
        };

        queryContext.Users.Add(user);
        await queryContext.SaveChangesAsync();

        var result = await repository.GetByIdAsync(userId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.UserConfig.Should().NotBeNull();
        result.UserConfig!.Language.Should().Be("en");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        var (repository, _, _) = CreateRepository();

        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WhenUserExists_ReturnsUser()
    {
        var (repository, queryContext, _) = CreateRepository();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "unique-user",
            Email = "name@example.com"
        };

        queryContext.Users.Add(user);
        await queryContext.SaveChangesAsync();

        var result = await repository.GetByNameAsync("unique-user", CancellationToken.None);

        result.Should().NotBeNull();
        result!.UserName.Should().Be("unique-user");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        var (repository, queryContext, _) = CreateRepository();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "email-user",
            Email = "email@test.local"
        };

        queryContext.Users.Add(user);
        await queryContext.SaveChangesAsync();

        var result = await repository.GetByEmailAsync("email@test.local", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be("email@test.local");
    }

    [Fact]
    public async Task UpdateAsync_WhenCalled_PersistsChangesUsingCommandContext()
    {
        var (repository, queryContext, commandContext) = CreateRepository();

        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "before",
            Email = "before@example.com"
        };

        queryContext.Users.Add(user);
        await queryContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(userId, CancellationToken.None);
        loaded.Should().NotBeNull();
        loaded!.UserName = "after";

        await repository.UpdateAsync(loaded, CancellationToken.None);

        var reloaded = await commandContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        reloaded.Should().NotBeNull();
        reloaded!.UserName.Should().Be("after");
    }

    [Fact]
    public async Task GetUserSolutionsAsync_WhenNoSolutionsForUser_ReturnsEmptyList()
    {
        var (repository, _, _) = CreateRepository();
        var userId = Guid.NewGuid();

        var results = await repository.GetUserSolutionsAsync(userId, 0, 10, CancellationToken.None);

        results.Should().NotBeNull();
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WhenQueryEmpty_ReturnsPagedUsersOrderedByUserName()
    {
        var (repository, queryContext, _) = CreateRepository();

        var userA = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "adam",
            Email = "adam@example.com"
        };

        var userB = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "zoe",
            Email = "zoe@example.com"
        };

        queryContext.Users.AddRange(userA, userB);
        await queryContext.SaveChangesAsync();

        var results = await repository.SearchAsync(string.Empty, 1, 10, CancellationToken.None);

        results.Should().HaveCount(2);
        results.Select(u => u.UserName).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchAsync_WhenQueryProvided_FiltersByUserNameOrEmailCaseInsensitive()
    {
        var (repository, queryContext, _) = CreateRepository();

        var user1 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "search-target",
            Email = "other@example.com"
        };

        var user2 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "someone-else",
            Email = "search@example.com"
        };

        var user3 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "unrelated",
            Email = "unrelated@example.com"
        };

        queryContext.Users.AddRange(user1, user2, user3);
        await queryContext.SaveChangesAsync();

        var results = await repository.SearchAsync("search", 1, 10, CancellationToken.None);

        results.Should().HaveCount(2);
        results.Select(u => u.Id).Should().Contain(new[] { user1.Id, user2.Id });
        results.Select(u => u.Id).Should().NotContain(user3.Id);
    }
}