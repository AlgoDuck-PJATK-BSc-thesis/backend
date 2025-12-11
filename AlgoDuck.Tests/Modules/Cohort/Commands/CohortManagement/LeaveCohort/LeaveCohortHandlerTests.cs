using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.LeaveCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.CohortManagement.LeaveCohort;

public class LeaveCohortHandlerTests
{
    static ApplicationCommandDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsCohortValidationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new LeaveCohortHandler(
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotInCohort_ThenThrowsCohortValidationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = null
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LeaveCohortHandler(
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFound_ThenThrowsCohortNotFoundException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LeaveCohortHandler(
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenLastMemberLeaves_ThenDeactivatesCohort()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        var cohort = new AlgoDuck.Models.Cohort
        {
            CohortId = cohortId,
            Name = "Test",
            CreatedByUserId = Guid.NewGuid(),
            IsActive = true
        };

        dbContext.Cohorts.Add(cohort);
        dbContext.ApplicationUsers.Add(user);
        dbContext.SaveChanges();

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Callback<ApplicationUser, CancellationToken>((u, _) =>
            {
                dbContext.ApplicationUsers.Update(u);
                dbContext.SaveChanges();
            })
            .Returns(Task.CompletedTask);

        var handler = new LeaveCohortHandler(
            userRepositoryMock.Object,
            dbContext);

        await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Null(user.CohortId);

        var reloadedCohort = await dbContext.Cohorts.FindAsync(cohortId);
        Assert.NotNull(reloadedCohort);
        Assert.False(reloadedCohort!.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenOtherMembersRemain_ThenKeepsCohortActive()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var leavingUser = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        var remainingUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            CohortId = cohortId
        };

        var cohort = new AlgoDuck.Models.Cohort
        {
            CohortId = cohortId,
            Name = "Test",
            CreatedByUserId = Guid.NewGuid(),
            IsActive = true
        };

        dbContext.Cohorts.Add(cohort);
        dbContext.ApplicationUsers.Add(leavingUser);
        dbContext.ApplicationUsers.Add(remainingUser);
        dbContext.SaveChanges();

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leavingUser);

        userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Callback<ApplicationUser, CancellationToken>((u, _) =>
            {
                dbContext.ApplicationUsers.Update(u);
                dbContext.SaveChanges();
            })
            .Returns(Task.CompletedTask);

        var handler = new LeaveCohortHandler(
            userRepositoryMock.Object,
            dbContext);

        await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Null(leavingUser.CohortId);

        var reloadedCohort = await dbContext.Cohorts.FindAsync(cohortId);
        Assert.NotNull(reloadedCohort);
        Assert.True(reloadedCohort!.IsActive);
    }
}