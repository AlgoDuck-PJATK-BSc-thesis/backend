using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.Admin.GetUsers;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.Admin.GetUsers;

public sealed class GetUsersHandlerTests
{
    static ApplicationQueryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_MapsUsersAndRoles_DistinctAndOrdered()
    {
        using var db = CreateDb();

        var adminRoleId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();

        db.Roles.Add(new IdentityRole<Guid> { Id = adminRoleId, Name = "admin", NormalizedName = "ADMIN" });
        db.Roles.Add(new IdentityRole<Guid> { Id = userRoleId, Name = "user", NormalizedName = "USER" });

        var u1 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@gmail.com" };
        var u2 = new ApplicationUser { Id = Guid.NewGuid(), UserName = null, Email = null };

        db.Users.Add(u1);
        db.Users.Add(u2);

        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = u1.Id, RoleId = userRoleId });
        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = u1.Id, RoleId = adminRoleId });

        await db.SaveChangesAsync(CancellationToken.None);

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetPagedAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ApplicationUser> { u1, u2 }, 10));

        var handler = new GetUsersHandler(repo.Object, db);

        var result = await handler.HandleAsync(new GetUsersDto { Page = 1, PageSize = 2 }, CancellationToken.None);

        Assert.Equal(1, result.CurrPage);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(10, result.TotalItems);
        Assert.Null(result.PrevCursor);
        Assert.Equal(2, result.NextCursor);

        var items = result.Items.ToList();
        Assert.Equal(2, items.Count);

        var item1 = items[0];
        Assert.Equal(u1.Id, item1.UserId);
        Assert.Equal("alice", item1.Username);
        Assert.Equal("alice@gmail.com", item1.Email);

        var roles1 = item1.Roles.ToList();
        Assert.Equal(2, roles1.Count);
        Assert.Equal("admin", roles1[0], StringComparer.OrdinalIgnoreCase);
        Assert.Equal("user", roles1[1], StringComparer.OrdinalIgnoreCase);

        var item2 = items[1];
        Assert.Equal(u2.Id, item2.UserId);
        Assert.Equal(string.Empty, item2.Username);
        Assert.Equal(string.Empty, item2.Email);

        var roles2 = item2.Roles.ToList();
        Assert.Empty(roles2);

        repo.Verify(x => x.GetPagedAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoUsers_ReturnsEmptyItemsAndNoNextCursor()
    {
        using var db = CreateDb();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(x => x.GetPagedAsync(3, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ApplicationUser>(), 120));

        var handler = new GetUsersHandler(repo.Object, db);

        var result = await handler.HandleAsync(new GetUsersDto { Page = 3, PageSize = 50 }, CancellationToken.None);

        Assert.Equal(3, result.CurrPage);
        Assert.Equal(50, result.PageSize);
        Assert.Equal(120, result.TotalItems);
        Assert.Equal(2, result.PrevCursor);
        Assert.Null(result.NextCursor);
        Assert.Empty(result.Items);

        repo.Verify(x => x.GetPagedAsync(3, 50, It.IsAny<CancellationToken>()), Times.Once);
    }
}
