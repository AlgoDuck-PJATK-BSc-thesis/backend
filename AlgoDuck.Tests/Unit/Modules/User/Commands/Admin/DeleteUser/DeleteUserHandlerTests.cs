using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.Admin.DeleteUser;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserValidationException = AlgoDuck.Modules.User.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.Admin.DeleteUser;

public sealed class DeleteUserHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    static ApplicationCommandDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        using var db = CreateDb();

        var handler = new DeleteUserHandler(userManager.Object, db);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(Guid.Empty, CancellationToken.None));

        Assert.Equal("User identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var userManager = CreateUserManagerMock();
        using var db = CreateDb();

        var userId = Guid.NewGuid();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var handler = new DeleteUserHandler(userManager.Object, db);

        var ex = await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));

        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserCreatedCohorts_SetsCreatedByUserIdNull_AndSetsLabelIfMissing()
    {
        var userManager = CreateUserManagerMock();
        using var db = CreateDb();

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = "alice@gmail.com",
            CohortId = null
        };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C1",
            JoinCode = "CODE",
            CreatedByUserId = userId,
            CreatedByUserLabel = null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteUserHandler(userManager.Object, db);

        await handler.HandleAsync(userId, CancellationToken.None);

        var updated = await db.Cohorts.AsTracking().Where(c => c.CreatedByUserId == null).ToListAsync();
        Assert.True(updated.Count == 1);
        Assert.Equal("alice", updated[0].CreatedByUserLabel);
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ThrowsValidationExceptionWithIdentityError()
    {
        var userManager = CreateUserManagerMock();
        using var db = CreateDb();

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = "alice@gmail.com",
            CohortId = null
        };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        var failed = IdentityResult.Failed(new IdentityError { Description = "Delete failed." });
        userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(failed);

        var handler = new DeleteUserHandler(userManager.Object, db);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));

        Assert.Equal("Delete failed.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortBecomesEmpty_SetsInactiveAndEmptiedAt()
    {
        var userManager = CreateUserManagerMock();
        using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "CODE",
            CreatedByUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmptiedAt = null
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = "alice@gmail.com",
            CohortId = cohortId
        };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteUserHandler(userManager.Object, db);

        await handler.HandleAsync(userId, CancellationToken.None);

        var cohort = await db.Cohorts.AsTracking().FirstAsync(c => c.CohortId == cohortId);
        Assert.False(cohort.IsActive);
        Assert.NotNull(cohort.EmptiedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortHasMembersAndWasInactive_ReactivatesAndClearsEmptiedAt()
    {
        var userManager = CreateUserManagerMock();
        using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "CODE",
            CreatedByUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            EmptiedAt = DateTime.UtcNow.AddDays(-1)
        });

        db.ApplicationUsers.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "remaining",
            Email = "r@b.com",
            CohortId = cohortId
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = "alice@gmail.com",
            CohortId = cohortId
        };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteUserHandler(userManager.Object, db);

        await handler.HandleAsync(userId, CancellationToken.None);

        var cohort = await db.Cohorts.AsTracking().FirstAsync(c => c.CohortId == cohortId);
        Assert.True(cohort.IsActive);
        Assert.Null(cohort.EmptiedAt);
    }
}
