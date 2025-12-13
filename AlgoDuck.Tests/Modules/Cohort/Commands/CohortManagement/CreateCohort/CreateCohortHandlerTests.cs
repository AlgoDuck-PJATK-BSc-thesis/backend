using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.CohortManagement.CreateCohort;

public class CreateCohortHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();

        var userId = Guid.NewGuid();
        var dto = new CreateCohortDto
        {
            Name = ""
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(CreateCohortDto.Name), "Required")
            }));

        var handler = new CreateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();

        var userId = Guid.NewGuid();
        var dto = new CreateCohortDto
        {
            Name = "My Cohort"
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new CreateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();

        var userId = Guid.NewGuid();
        var dto = new CreateCohortDto
        {
            Name = "My Cohort"
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

        var handler = new CreateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenReturnsResultAndUpdatesUser()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();

        var userId = Guid.NewGuid();
        var dto = new CreateCohortDto
        {
            Name = "My Cohort"
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

        userRepositoryMock
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(userId, result.CreatedByUserId);
        Assert.True(result.IsActive);
        Assert.NotEqual(Guid.Empty, result.CohortId);

        Assert.Equal(result.CohortId, user.CohortId);
        userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}