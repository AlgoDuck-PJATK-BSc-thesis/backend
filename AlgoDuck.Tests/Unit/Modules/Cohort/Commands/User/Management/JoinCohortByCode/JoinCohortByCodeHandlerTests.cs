using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohortByCode;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.JoinCohortByCode;

public sealed class JoinCohortByCodeHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsCohortValidationException()
    {
        var cohortRepo = new Mock<ICohortRepository>();
        var userRepo = new Mock<IUserRepository>();
        var handler = new JoinCohortByCodeHandler(new JoinCohortByCodeValidator(), cohortRepo.Object, userRepo.Object);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new JoinCohortByCodeDto { Code = "" }, CancellationToken.None));

        Assert.Equal("Invalid join code.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsCohortValidationException()
    {
        var cohortRepo = new Mock<ICohortRepository>();
        var userRepo = new Mock<IUserRepository>();
        var handler = new JoinCohortByCodeHandler(new JoinCohortByCodeValidator(), cohortRepo.Object, userRepo.Object);

        var userId = Guid.NewGuid();
        userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((ApplicationUser?)null);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, new JoinCohortByCodeDto { Code = "ABC123" }, CancellationToken.None));

        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyInCohort_ThrowsCohortValidationException()
    {
        var cohortRepo = new Mock<ICohortRepository>();
        var userRepo = new Mock<IUserRepository>();
        var handler = new JoinCohortByCodeHandler(new JoinCohortByCodeValidator(), cohortRepo.Object, userRepo.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, CohortId = Guid.NewGuid() };

        userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, new JoinCohortByCodeDto { Code = "ABC123" }, CancellationToken.None));

        Assert.Equal("Leave current cohort to join another.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFound_ThrowsCohortValidationException()
    {
        var cohortRepo = new Mock<ICohortRepository>();
        var userRepo = new Mock<IUserRepository>();
        var handler = new JoinCohortByCodeHandler(new JoinCohortByCodeValidator(), cohortRepo.Object, userRepo.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, CohortId = null };

        userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        cohortRepo.Setup(x => x.GetByJoinCodeAsync("ABC123", It.IsAny<CancellationToken>())).ReturnsAsync((Models.Cohort?)null);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, new JoinCohortByCodeDto { Code = "ABC123" }, CancellationToken.None));

        Assert.Equal("Cohort not found.", ex.Message);

        cohortRepo.Verify(x => x.GetByJoinCodeAsync("ABC123", It.IsAny<CancellationToken>()), Times.Once);
        userRepo.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortInactive_ThrowsCohortValidationException()
    {
        var cohortRepo = new Mock<ICohortRepository>();
        var userRepo = new Mock<IUserRepository>();
        var handler = new JoinCohortByCodeHandler(new JoinCohortByCodeValidator(), cohortRepo.Object, userRepo.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, CohortId = null };

        userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var cohortId = Guid.NewGuid();
        var cohort = new Models.Cohort { CohortId = cohortId, Name = "Test Cohort", JoinCode = "ABC123", IsActive = false };

        cohortRepo.Setup(x => x.GetByJoinCodeAsync("ABC123", It.IsAny<CancellationToken>())).ReturnsAsync(cohort);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, new JoinCohortByCodeDto { Code = "ABC123" }, CancellationToken.None));

        Assert.Equal("Cohort not found.", ex.Message);

        userRepo.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccess_UpdatesUserAndReturnsResult()
    {
        var cohortRepo = new Mock<ICohortRepository>();
        var userRepo = new Mock<IUserRepository>();
        var handler = new JoinCohortByCodeHandler(new JoinCohortByCodeValidator(), cohortRepo.Object, userRepo.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, CohortId = null };

        userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        userRepo.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var cohortId = Guid.NewGuid();
        var cohort = new Models.Cohort { CohortId = cohortId, Name = "Test Cohort", JoinCode = "ABC123", IsActive = true };

        cohortRepo.Setup(x => x.GetByJoinCodeAsync("ABC123", It.IsAny<CancellationToken>())).ReturnsAsync(cohort);

        var result = await handler.HandleAsync(userId, new JoinCohortByCodeDto { Code = "ABC123" }, CancellationToken.None);

        Assert.Equal(cohortId, user.CohortId);

        cohortRepo.Verify(x => x.GetByJoinCodeAsync("ABC123", It.IsAny<CancellationToken>()), Times.Once);
        userRepo.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal("Test Cohort", result.Name);
        Assert.True(result.IsActive);
    }
}
