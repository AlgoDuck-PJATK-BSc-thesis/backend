using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.UpdateCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.CohortManagement.UpdateCohort;

public class UpdateCohortHandlerTests
{
    static ApplicationCommandDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = ""
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(UpdateCohortDto.Name), "Required")
            }));

        var handler = new UpdateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, dto, CancellationToken.None));

        userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = "New Name"
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new UpdateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = "New Name"
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = null
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserInDifferentCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = "New Name"
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = Guid.NewGuid()
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFound_ThenThrowsCohortNotFoundException()
    {
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = "New Name"
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(userId, cohortId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenCohortInactive_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = "New Name"
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        var cohort = new AlgoDuck.Models.Cohort
        {
            CohortId = cohortId,
            Name = "Old Name",
            JoinCode = "UPD00001",
            CreatedByUserId = Guid.NewGuid(),
            IsActive = false
        };

        dbContext.Cohorts.Add(cohort);
        await dbContext.SaveChangesAsync();

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenUpdatesCohortAndReturnsResult()
    {
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = "New Name"
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        var cohort = new AlgoDuck.Models.Cohort
        {
            CohortId = cohortId,
            Name = "Old Name",
            JoinCode = "UPD00002",
            CreatedByUserId = Guid.NewGuid(),
            IsActive = true
        };

        dbContext.Cohorts.Add(cohort);
        await dbContext.SaveChangesAsync();

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            dbContext);

        var result = await handler.HandleAsync(userId, cohortId, dto, CancellationToken.None);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(cohort.CreatedByUserId, result.CreatedByUserId);
        Assert.True(result.IsActive);

        var reloadedCohort = await dbContext.Cohorts.FindAsync(cohortId);
        Assert.NotNull(reloadedCohort);
        Assert.Equal(dto.Name, reloadedCohort.Name);
    }
}