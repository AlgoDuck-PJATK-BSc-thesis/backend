using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortMembers;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Modules.Cohort.Queries.GetCohortMembers;

public class GetCohortMembersHandlerTests
{
    static ApplicationQueryDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenRequestInvalid_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<GetCohortMembersRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var profileServiceMock = new Mock<IProfileService>();
        await using var dbContext = CreateInMemoryContext();

        var dto = new GetCohortMembersRequestDto
        {
            CohortId = Guid.Empty,
            Page = 0,
            PageSize = 0
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(GetCohortMembersRequestDto.CohortId), "Invalid")
            }));

        var handler = new GetCohortMembersHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            profileServiceMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<GetCohortMembersRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var profileServiceMock = new Mock<IProfileService>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortMembersRequestDto
        {
            CohortId = cohortId,
            Page = 1,
            PageSize = 10
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new GetCohortMembersHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            profileServiceMock.Object,
            dbContext);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenMembersExist_ThenReturnsPagedProfiles()
    {
        var validatorMock = new Mock<IValidator<GetCohortMembersRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var profileServiceMock = new Mock<IProfileService>();
        await using var dbContext = CreateInMemoryContext();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortMembersRequestDto
        {
            CohortId = cohortId,
            Page = 1,
            PageSize = 2
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var user1 = new ApplicationUser
        {
            Id = userId,
            CohortId = cohortId,
            UserName = "duck1"
        };

        var user2 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            CohortId = cohortId,
            UserName = "duck2"
        };

        dbContext.ApplicationUsers.Add(user1);
        dbContext.ApplicationUsers.Add(user2);
        dbContext.SaveChanges();

        profileServiceMock
            .Setup(p => p.GetProfileAsync(user1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto
            {
                UserId = user1.Id,
                Username = user1.UserName,
                S3AvatarUrl = "url1"
            });

        profileServiceMock
            .Setup(p => p.GetProfileAsync(user2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto
            {
                UserId = user2.Id,
                Username = user2.UserName,
                S3AvatarUrl = "url2"
            });

        var handler = new GetCohortMembersHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            profileServiceMock.Object,
            dbContext);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(user1.Id, result.Items[0].UserId);
        Assert.True(result.Items[0].IsYou);
        Assert.Equal(user2.Id, result.Items[1].UserId);
        Assert.False(result.Items[1].IsYou);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
    }
}