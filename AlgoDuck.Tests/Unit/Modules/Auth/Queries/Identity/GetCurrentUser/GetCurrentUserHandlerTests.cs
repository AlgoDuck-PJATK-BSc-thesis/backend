using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Queries.Identity.GetCurrentUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Queries.Identity.GetCurrentUser;

public sealed class GetCurrentUserHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>> { new Mock<IUserValidator<ApplicationUser>>().Object };
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new Mock<IPasswordValidator<ApplicationUser>>().Object };
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services.Object,
            logger.Object
        );
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
