using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetUserCohorts;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.User.Cohorts.GetUserCohorts;

public sealed class GetUserCohortsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ThenThrowsCohortValidationException()
    {
        var userId = Guid.NewGuid();

        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserCohortsHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(
            () => handler.HandleAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoCohort_ThenReturnsEmptyList()
    {
        var userId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = null
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetUserCohortsHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasCohortButRepositoryReturnsNone_ThenReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        cohortRepositoryMock
            .Setup(r => r.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.Cohort?)null);

        var handler = new GetUserCohortsHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasActiveCohort_ThenReturnsSingleItem()
    {
        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Test Cohort",
            IsActive = true,
            CreatedByUserId = creatorId
        };

        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        cohortRepositoryMock
            .Setup(r => r.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cohort);

        var handler = new GetUserCohortsHandler(
            userRepositoryMock.Object,
            cohortRepositoryMock.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        var item = Assert.Single(result.Items);
        Assert.Equal(cohortId, item.CohortId);
        Assert.Equal("Test Cohort", item.Name);
        Assert.True(item.IsActive);
        Assert.Equal(creatorId, item.CreatedByUserId);
    }
}