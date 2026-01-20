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
                UserId = userId
            }
        };

        queryContext.Users.Add(user);
        await queryContext.SaveChangesAsync();

        var result = await repository.GetByIdAsync(userId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.UserConfig.Should().NotBeNull();
        result.UserConfig!.UserId.Should().Be(userId);
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
        result.UserName.Should().Be("unique-user");
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
        result.Email.Should().Be("email@test.local");
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
        loaded.UserName = "after";

        await repository.UpdateAsync(loaded, CancellationToken.None);

        var reloaded = await commandContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        reloaded.Should().NotBeNull();
        reloaded.UserName.Should().Be("after");
    }
}
