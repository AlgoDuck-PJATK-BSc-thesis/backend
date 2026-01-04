using AlgoDuck.Modules.User.Queries.User.Profile.GetSelectedAvatar;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.GetSelectedAvatar;

public sealed class GetSelectedAvatarHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var profileService = new Mock<IProfileService>();
        var handler = new GetSelectedAvatarHandler(profileService.Object);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenReturnsSelectedAvatarDto()
    {
        var userId = Guid.NewGuid();

        var profileService = new Mock<IProfileService>();
        profileService.Setup(x => x.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto
            {
                UserId = userId,
                S3AvatarUrl = "s3://avatar.png"
            });

        var handler = new GetSelectedAvatarHandler(profileService.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.Equal("s3://avatar.png", result.S3AvatarUrl);

        profileService.Verify(x => x.GetProfileAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}