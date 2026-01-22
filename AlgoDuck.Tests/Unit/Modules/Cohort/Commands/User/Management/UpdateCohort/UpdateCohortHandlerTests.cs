using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.UpdateCohort;

public class UpdateCohortHandlerTests
{
    static ApplicationCommandDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

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
        var validatorMock = new Mock<IValidator<UpdateCohortDto>>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var moderationMock = CreateModerationAllowAllMock();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UpdateCohortDto
        {
            Name = "ab"
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
            JoinCode = "UPD00003",
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
            dbContext,
            moderationMock.Object);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, cohortId, dto, CancellationToken.None));

        Assert.Equal("Cohort name must be at least 3 characters.", ex.Message);
    }
}
