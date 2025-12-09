using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public class PermissionsServiceTests
{
    [Fact]
    public async Task EnsureUserHasPermissionAsync_WhenUserIdIsEmpty_ThenThrowsPermissionException()
    {
        var repositoryMock = new Mock<IPermissionsRepository>();
        var service = new PermissionsService(repositoryMock.Object);

        await Assert.ThrowsAsync<PermissionException>(() =>
            service.EnsureUserHasPermissionAsync(Guid.Empty, "auth.read", CancellationToken.None));

        repositoryMock.Verify(x => x.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureUserHasPermissionAsync_WhenPermissionIsNullOrWhitespace_ThenThrowsPermissionException()
    {
        var repositoryMock = new Mock<IPermissionsRepository>();
        var service = new PermissionsService(repositoryMock.Object);
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<PermissionException>(() =>
            service.EnsureUserHasPermissionAsync(userId, "", CancellationToken.None));

        await Assert.ThrowsAsync<PermissionException>(() =>
            service.EnsureUserHasPermissionAsync(userId, "   ", CancellationToken.None));

        repositoryMock.Verify(x => x.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureUserHasPermissionAsync_WhenUserHasRequiredPermission_ThenCompletesWithoutException()
    {
        var repositoryMock = new Mock<IPermissionsRepository>();
        var service = new PermissionsService(repositoryMock.Object);
        var userId = Guid.NewGuid();
        var requiredPermission = "auth.manage";

        var userPermissions = new List<string>
        {
            "auth.read",
            "auth.manage",
            "user.profile"
        };

        repositoryMock
            .Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        var exception = await Record.ExceptionAsync(() =>
            service.EnsureUserHasPermissionAsync(userId, requiredPermission, CancellationToken.None));

        Assert.Null(exception);

        repositoryMock.Verify(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureUserHasPermissionAsync_WhenUserDoesNotHaveRequiredPermission_ThenThrowsPermissionException()
    {
        var repositoryMock = new Mock<IPermissionsRepository>();
        var service = new PermissionsService(repositoryMock.Object);
        var userId = Guid.NewGuid();
        var requiredPermission = "auth.manage";

        var userPermissions = new List<string>
        {
            "auth.read",
            "user.profile"
        };

        repositoryMock
            .Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        await Assert.ThrowsAsync<PermissionException>(() =>
            service.EnsureUserHasPermissionAsync(userId, requiredPermission, CancellationToken.None));

        repositoryMock.Verify(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
