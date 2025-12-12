using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.GetTwoFactorEnabled;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.GetTwoFactorEnabled;

public sealed class GetTwoFactorEnabledHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var userRepository = new Mock<IUserRepository>();
        var handler = new GetTwoFactorEnabledHandler(userRepository.Object);

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

        var handler = new GetTwoFactorEnabledHandler(userRepository.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ThenReturnsTwoFactorStatusDto()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                TwoFactorEnabled = true,
                UserName = "u1",
                Email = "u1@test.local",
                SecurityStamp = Guid.NewGuid().ToString()
            });

        var handler = new GetTwoFactorEnabledHandler(userRepository.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.True(result.TwoFactorEnabled);
    }
}