using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.User.Management.CreateCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.CreateCohort;

public class CreateCohortHandlerTests
{
    static ChatModerationResult MakeModerationResult(bool isAllowed, string? blockReason, string? category)
    {
        var t = typeof(ChatModerationResult);

        var ctor = t.GetConstructor(new[] { typeof(bool), typeof(string), typeof(string) });
        if (ctor is not null)
        {
            return (ChatModerationResult)ctor.Invoke(new object?[] { isAllowed, blockReason, category });
        }

        var instance = Activator.CreateInstance(t);
        if (instance is null)
        {
            throw new InvalidOperationException("Could not create ChatModerationResult instance.");
        }

        var pAllowed = t.GetProperty("IsAllowed");
        var pReason = t.GetProperty("BlockReason");
        var pCategory = t.GetProperty("Category");

        if (pAllowed is not null) pAllowed.SetValue(instance, isAllowed);
        if (pReason is not null) pReason.SetValue(instance, blockReason);
        if (pCategory is not null) pCategory.SetValue(instance, category);

        return (ChatModerationResult)instance;
    }

    static Mock<IChatModerationService> CreateModerationAllowAllMock()
    {
        var moderationMock = new Mock<IChatModerationService>();

        moderationMock
            .Setup(x => x.CheckMessageAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeModerationResult(true, null, null));

        return moderationMock;
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var moderationMock = CreateModerationAllowAllMock();

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
            userRepositoryMock.Object,
            cohortRepositoryMock.Object,
            moderationMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        cohortRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AlgoDuck.Models.Cohort>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var moderationMock = CreateModerationAllowAllMock();

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
            userRepositoryMock.Object,
            cohortRepositoryMock.Object,
            moderationMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        cohortRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AlgoDuck.Models.Cohort>(), It.IsAny<CancellationToken>()), Times.Never);
        userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var moderationMock = CreateModerationAllowAllMock();

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
            userRepositoryMock.Object,
            cohortRepositoryMock.Object,
            moderationMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        cohortRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AlgoDuck.Models.Cohort>(), It.IsAny<CancellationToken>()), Times.Never);
        userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenReturnsResultAndUpdatesUser()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var moderationMock = CreateModerationAllowAllMock();

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

        cohortRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AlgoDuck.Models.Cohort>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            cohortRepositoryMock.Object,
            moderationMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(userId, result.CreatedByUserId);
        Assert.True(result.IsActive);
        Assert.NotEqual(Guid.Empty, result.CohortId);

        Assert.Equal(result.CohortId, user.CohortId);

        cohortRepositoryMock.Verify(
            x => x.AddAsync(It.Is<AlgoDuck.Models.Cohort>(c => c.CohortId == result.CohortId), It.IsAny<CancellationToken>()),
            Times.Once);

        userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
