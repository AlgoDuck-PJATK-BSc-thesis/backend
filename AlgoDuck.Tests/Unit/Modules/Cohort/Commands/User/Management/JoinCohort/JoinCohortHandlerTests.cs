using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.JoinCohort;

public class JoinCohortHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsCohortValidationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new JoinCohortHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyInCohort_ThenThrowsCohortValidationException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = Guid.NewGuid()
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new JoinCohortHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFound_ThenThrowsCohortNotFoundException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = null
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        cohortRepositoryMock
            .Setup(x => x.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.Cohort?)null);

        var handler = new JoinCohortHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(userId, cohortId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenCohortInactive_ThenThrowsCohortNotFoundException()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = null
        };

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Test",
            CreatedByUserId = Guid.NewGuid(),
            IsActive = false
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        cohortRepositoryMock
            .Setup(x => x.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cohort);

        var handler = new JoinCohortHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(userId, cohortId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenUpdatesUserAndReturnsResult()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = null
        };

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Test Cohort",
            CreatedByUserId = Guid.NewGuid(),
            IsActive = true
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        cohortRepositoryMock
            .Setup(x => x.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cohort);

        userRepositoryMock
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new JoinCohortHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        var result = await handler.HandleAsync(userId, cohortId, CancellationToken.None);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal(cohort.Name, result.Name);
        Assert.Equal(cohort.CreatedByUserId, result.CreatedByUserId);
        Assert.True(result.IsActive);

        Assert.Equal(cohortId, user.CohortId);
        userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}