using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Profile.GetVerifiedEmail;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Profile.GetVerifiedEmail;

public sealed class GetVerifiedEmailHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var userRepository = new Mock<IUserRepository>();
        var handler = new GetVerifiedEmailHandler(userRepository.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetVerifiedEmailHandler(userRepository.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserFound_ThenReturnsEmailConfirmed()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                UserName = "u1",
                Email = "u1@test.local",
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true
            });

        var handler = new GetVerifiedEmailHandler(userRepository.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.True(result.EmailConfirmed);
    }
}