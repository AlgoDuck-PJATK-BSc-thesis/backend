using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetCohortById;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.User.Cohorts.GetCohortById;

public sealed class GetCohortByIdHandlerTests
{
    private static GetCohortByIdHandler CreateHandler(
        out Mock<IValidator<GetCohortByIdRequestDto>> validatorMock,
        out Mock<ICohortRepository> cohortRepositoryMock,
        out Mock<IUserRepository> userRepositoryMock)
    {
        validatorMock = new Mock<IValidator<GetCohortByIdRequestDto>>();
        cohortRepositoryMock = new Mock<ICohortRepository>();
        userRepositoryMock = new Mock<IUserRepository>();

        return new GetCohortByIdHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenRequestInvalid_ThenThrowsCohortValidationException()
    {
        var handler = CreateHandler(
            out var validatorMock,
            out var cohortRepositoryMock,
            out var userRepositoryMock);

        var userId = Guid.NewGuid();
        var dto = new GetCohortByIdRequestDto
        {
            CohortId = Guid.Empty
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(GetCohortByIdRequestDto.CohortId), "Invalid")
            }));

        await Assert.ThrowsAsync<CohortValidationException>(
            () => handler.HandleAsync(userId, dto, CancellationToken.None));

        cohortRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        userRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFoundOrInactive_ThenThrowsCohortNotFoundException()
    {
        var handler = CreateHandler(
            out var validatorMock,
            out var cohortRepositoryMock,
            out var userRepositoryMock);

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortByIdRequestDto
        {
            CohortId = cohortId
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.Cohort?)null);

        await Assert.ThrowsAsync<CohortNotFoundException>(
            () => handler.HandleAsync(userId, dto, CancellationToken.None));

        userRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortExistsAndUserIsMember_ThenReturnsResultWithIsMemberTrue()
    {
        var handler = CreateHandler(
            out var validatorMock,
            out var cohortRepositoryMock,
            out var userRepositoryMock);

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var dto = new GetCohortByIdRequestDto
        {
            CohortId = cohortId
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Target Cohort",
            IsActive = true,
            CreatedByUserId = creatorId
        };

        cohortRepositoryMock
            .Setup(r => r.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cohort);

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal("Target Cohort", result.Name);
        Assert.True(result.IsActive);
        Assert.Equal(creatorId, result.CreatedByUserId);
        Assert.True(result.IsMember);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortExistsAndUserIsNotMember_ThenReturnsResultWithIsMemberFalse()
    {
        var handler = CreateHandler(
            out var validatorMock,
            out var cohortRepositoryMock,
            out var userRepositoryMock);

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var dto = new GetCohortByIdRequestDto
        {
            CohortId = cohortId
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Target Cohort",
            IsActive = true,
            CreatedByUserId = creatorId
        };

        cohortRepositoryMock
            .Setup(r => r.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cohort);

        var user = new ApplicationUser
        {
            Id = userId,
            CohortId = Guid.NewGuid()
        };

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal("Target Cohort", result.Name);
        Assert.True(result.IsActive);
        Assert.Equal(creatorId, result.CreatedByUserId);
        Assert.False(result.IsMember);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsCohortValidationException()
    {
        var handler = CreateHandler(
            out var validatorMock,
            out var cohortRepositoryMock,
            out var userRepositoryMock);

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortByIdRequestDto
        {
            CohortId = cohortId
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Target Cohort",
            IsActive = true,
            CreatedByUserId = Guid.NewGuid()
        };

        cohortRepositoryMock
            .Setup(r => r.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cohort);

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<CohortValidationException>(
            () => handler.HandleAsync(userId, dto, CancellationToken.None));
    }
}