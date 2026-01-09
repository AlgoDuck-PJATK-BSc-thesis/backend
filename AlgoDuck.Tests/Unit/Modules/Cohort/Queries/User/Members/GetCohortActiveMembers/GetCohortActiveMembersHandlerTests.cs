using AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortActiveMembers;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.User.Members.GetCohortActiveMembers;

public class GetCohortActiveMembersHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRequestInvalid_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<GetCohortActiveMembersRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var presenceServiceMock = new Mock<IChatPresenceService>();
        var profileServiceMock = new Mock<IProfileService>();

        var dto = new GetCohortActiveMembersRequestDto
        {
            CohortId = Guid.Empty
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(GetCohortActiveMembersRequestDto.CohortId), "Invalid")
            }));

        var handler = new GetCohortActiveMembersHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            presenceServiceMock.Object,
            profileServiceMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<GetCohortActiveMembersRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var presenceServiceMock = new Mock<IChatPresenceService>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortActiveMembersRequestDto
        {
            CohortId = cohortId
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new GetCohortActiveMembersHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            presenceServiceMock.Object,
            profileServiceMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenNoActiveUsers_ThenReturnsEmptyItems()
    {
        var validatorMock = new Mock<IValidator<GetCohortActiveMembersRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var presenceServiceMock = new Mock<IChatPresenceService>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortActiveMembersRequestDto
        {
            CohortId = cohortId
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        presenceServiceMock
            .Setup(p => p.GetActiveUsersAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CohortActiveUser>());

        var handler = new GetCohortActiveMembersHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            presenceServiceMock.Object,
            profileServiceMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task HandleAsync_WhenActiveUsersExist_ThenReturnsMappedProfiles()
    {
        var validatorMock = new Mock<IValidator<GetCohortActiveMembersRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var presenceServiceMock = new Mock<IChatPresenceService>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var activeUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var dto = new GetCohortActiveMembersRequestDto
        {
            CohortId = cohortId
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        presenceServiceMock
            .Setup(p => p.GetActiveUsersAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CohortActiveUser>
            {
                new CohortActiveUser
                {
                    UserId = activeUserId,
                    LastSeenAt = now
                }
            });

        profileServiceMock
            .Setup(p => p.GetProfileAsync(activeUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto
            {
                UserId = activeUserId,
                Username = "duck",
                S3AvatarUrl = "https://example/avatar.png"
            });

        var handler = new GetCohortActiveMembersHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            presenceServiceMock.Object,
            profileServiceMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Single(result.Items);
        var item = result.Items[0];
        Assert.Equal(activeUserId, item.UserId);
        Assert.Equal("duck", item.UserName);
        Assert.Equal("https://example/avatar.png", item.UserAvatarUrl);
        Assert.Equal(now, item.LastSeenAt);
        Assert.True(item.IsActive);
    }
}