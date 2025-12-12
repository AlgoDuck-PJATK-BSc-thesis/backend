using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ReturnsNull()
    {
        var userManager = CreateUserManagerMock();
        var handler = new GetCurrentUserHandler(userManager.Object);

        var result = await handler.HandleAsync(Guid.Empty, CancellationToken.None);

        Assert.Null(result);
        userManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsNull()
    {
        var userManager = CreateUserManagerMock();
        var handler = new GetCurrentUserHandler(userManager.Object);

        var userId = Guid.NewGuid();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.Null(result);
        userManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserFound_MapsToAuthUserDto()
    {
        var userManager = CreateUserManagerMock();
        var handler = new GetCurrentUserHandler(userManager.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = null,
            Email = null,
            EmailConfirmed = true
        };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result!.Id);
        Assert.Equal(string.Empty, result.UserName);
        Assert.Equal(string.Empty, result.Email);
        Assert.True(result.EmailConfirmed);
    }
}
