using AlgoDuck.Modules.User.Queries.User.Profile.GetUserProfile;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Profile.GetUserProfile;

public sealed class GetUserProfileHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenCalled_ThenDelegatesToProfileService()
    {
        var userId = Guid.NewGuid();

        var expected = new UserProfileDto
        {
            UserId = userId,
            S3AvatarUrl = "s3://avatar.png",
            Username = "u1",
            Language = "en",
            Coins = 5,
            Experience = 10,
            AmountSolved = 2,
            CohortId = null
        };

        var profileService = new Mock<IProfileService>();
        profileService.Setup(x => x.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetUserProfileHandler(profileService.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Same(expected, result);
        profileService.Verify(x => x.GetProfileAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}