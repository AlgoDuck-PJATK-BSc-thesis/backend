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
    public async Task HandleAsync_WhenNameTooShort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<CreateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var moderationMock = CreateModerationAllowAllMock();

        var userId = Guid.NewGuid();
        var dto = new CreateCohortDto
        {
            Name = "ab"
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

        var handler = new CreateCohortHandler(
            validatorMock.Object,
            userRepositoryMock.Object,
            cohortRepositoryMock.Object,
            moderationMock.Object);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Equal("Cohort name must be at least 3 characters.", ex.Message);
    }
}
