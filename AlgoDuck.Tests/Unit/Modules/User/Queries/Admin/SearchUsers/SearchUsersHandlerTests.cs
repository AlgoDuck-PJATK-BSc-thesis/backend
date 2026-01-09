using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.Admin.SearchUsers;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.Admin.SearchUsers;

public sealed class SearchUsersHandlerTests
{
    static ApplicationQueryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenQueryIsGuid_ReturnsIdMatchAndRoleEnrichment()
    {
        using var db = CreateDb();

        var adminRoleId = Guid.NewGuid();
        db.Roles.Add(new IdentityRole<Guid> { Id = adminRoleId, Name = "admin", NormalizedName = "ADMIN" });

        var idUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "id_user", Email = "id@b.com" };
        var uNameUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "john", Email = "john@b.com" };
        var emailUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "eve", Email = "eve@b.com" };

        db.Users.Add(idUser);
        db.Users.Add(uNameUser);
        db.Users.Add(emailUser);

        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = idUser.Id, RoleId = adminRoleId });
        await db.SaveChangesAsync(CancellationToken.None);

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);

        repo.Setup(x => x.GetByIdAsync(idUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idUser);

        repo.Setup(x => x.SearchByUsernamePagedAsync(idUser.Id.ToString(), 1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ApplicationUser> { uNameUser }, 5));

        repo.Setup(x => x.SearchByEmailPagedAsync(idUser.Id.ToString(), 2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ApplicationUser> { emailUser }, 7));

        var handler = new SearchUsersHandler(repo.Object, db);

        var result = await handler.HandleAsync(new SearchUsersDto
        {
            Query = idUser.Id.ToString(),
            UsernamePage = 1,
            UsernamePageSize = 2,
            EmailPage = 2,
            EmailPageSize = 3
        }, CancellationToken.None);

        Assert.NotNull(result.IdMatch);
        Assert.Equal(idUser.Id, result.IdMatch!.UserId);
        Assert.Equal("id_user", result.IdMatch.Username);
        Assert.Equal("id@b.com", result.IdMatch.Email);
        Assert.Single(result.IdMatch.Roles);
        Assert.Equal("admin", result.IdMatch.Roles[0], ignoreCase: true);

        Assert.Equal(1, result.Username.CurrPage);
        Assert.Equal(2, result.Username.PageSize);
        Assert.Equal(5, result.Username.TotalItems);
        Assert.Null(result.Username.PrevCursor);
        Assert.Equal(2, result.Username.NextCursor);
        Assert.Single(result.Username.Items);

        Assert.Equal(2, result.Email.CurrPage);
        Assert.Equal(3, result.Email.PageSize);
        Assert.Equal(7, result.Email.TotalItems);
        Assert.Equal(1, result.Email.PrevCursor);
        Assert.Equal(3, result.Email.NextCursor);
        Assert.Single(result.Email.Items);

        repo.Verify(x => x.GetByIdAsync(idUser.Id, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SearchByUsernamePagedAsync(idUser.Id.ToString(), 1, 2, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SearchByEmailPagedAsync(idUser.Id.ToString(), 2, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenQueryIsNotGuid_DoesNotCallGetByIdAsync_AndIdMatchNull()
    {
        using var db = CreateDb();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);

        repo.Setup(x => x.SearchByUsernamePagedAsync("john", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ApplicationUser>(), 0));

        repo.Setup(x => x.SearchByEmailPagedAsync("john", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ApplicationUser>(), 0));

        var handler = new SearchUsersHandler(repo.Object, db);

        var result = await handler.HandleAsync(new SearchUsersDto
        {
            Query = " john ",
            UsernamePage = 1,
            UsernamePageSize = 20,
            EmailPage = 1,
            EmailPageSize = 20
        }, CancellationToken.None);

        Assert.Null(result.IdMatch);
        Assert.Empty(result.Username.Items);
        Assert.Empty(result.Email.Items);

        repo.Verify(x => x.SearchByUsernamePagedAsync("john", 1, 20, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SearchByEmailPagedAsync("john", 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}
