using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Profile.GetUserById;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Profile.GetUserById;

public sealed class GetUserByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserByIdHandler(userRepository.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(new GetUserByIdRequestDto { UserId = Guid.NewGuid() }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoConfig_ThenReturnsDtoWithEmptyLanguage()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                UserName = "u1",
                Email = "u1@test.local",
                Coins = 5,
                Experience = 10,
                AmountSolved = 2,
                CohortId = null,
                UserConfig = null,
                SecurityStamp = Guid.NewGuid().ToString()
            });

        var handler = new GetUserByIdHandler(userRepository.Object);

        var result = await handler.HandleAsync(new GetUserByIdRequestDto { UserId = userId }, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.Equal("u1", result.Username);
        Assert.Equal("u1@test.local", result.Email);
        Assert.Equal(5, result.Coins);
        Assert.Equal(10, result.Experience);
        Assert.Equal(2, result.AmountSolved);
        Assert.Null(result.CohortId);
        Assert.Equal(string.Empty, result.Language);
        Assert.Equal(string.Empty, result.S3AvatarUrl);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasConfig_ThenMapsLanguage()
    {
        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = userId,
                UserName = "u1",
                Email = "u1@test.local",
                Coins = 5,
                Experience = 10,
                AmountSolved = 2,
                CohortId = cohortId,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserConfig = new UserConfig
                {
                    UserId = userId,
                    Language = "pl",
                    User = null!
                }
            });

        var handler = new GetUserByIdHandler(userRepository.Object);

        var result = await handler.HandleAsync(new GetUserByIdRequestDto { UserId = userId }, CancellationToken.None);

        Assert.Equal("pl", result.Language);
        Assert.Equal(cohortId, result.CohortId);
    }
}