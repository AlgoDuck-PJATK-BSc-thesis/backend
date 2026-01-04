using AlgoDuck.Modules.Auth.Queries.Permissions.CheckUserPermissions;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Queries.CheckUserPermissions;

public sealed class CheckUserPermissionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsPermissionException()
    {
        var permissionsService = new Mock<IPermissionsService>();
        var handler = new CheckUserPermissionsHandler(permissionsService.Object);

        var ex = await Assert.ThrowsAsync<PermissionException>(() =>
            handler.HandleAsync(Guid.Empty, new PermissionsDto { Permissions = new[] { "auth.read" } }, CancellationToken.None));

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("User identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenNoPermissions_ReturnsEmptyDictionary()
    {
        var permissionsService = new Mock<IPermissionsService>();
        var handler = new CheckUserPermissionsHandler(permissionsService.Object);

        var result = await handler.HandleAsync(Guid.NewGuid(), new PermissionsDto { Permissions = Array.Empty<string>() }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
        permissionsService.Verify(x => x.EnsureUserHasPermissionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_SkipsWhitespacePermissions_AndEvaluatesNonEmpty()
    {
        var permissionsService = new Mock<IPermissionsService>();
        var handler = new CheckUserPermissionsHandler(permissionsService.Object);

        var userId = Guid.NewGuid();
        var dto = new PermissionsDto { Permissions = new[] { " ", "auth.read", "\t", "auth.manage" } };

        permissionsService
            .Setup(x => x.EnsureUserHasPermissionAsync(userId, "auth.read", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        permissionsService
            .Setup(x => x.EnsureUserHasPermissionAsync(userId, "auth.manage", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PermissionException("no"));

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.True(result["auth.read"]);
        Assert.False(result["auth.manage"]);

        permissionsService.Verify(x => x.EnsureUserHasPermissionAsync(userId, "auth.read", It.IsAny<CancellationToken>()), Times.Once);
        permissionsService.Verify(x => x.EnsureUserHasPermissionAsync(userId, "auth.manage", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UsesCaseInsensitiveDictionary()
    {
        var permissionsService = new Mock<IPermissionsService>();
        var handler = new CheckUserPermissionsHandler(permissionsService.Object);

        var userId = Guid.NewGuid();

        permissionsService
            .Setup(x => x.EnsureUserHasPermissionAsync(userId, "AUTH.READ", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.HandleAsync(userId, new PermissionsDto { Permissions = new[] { "AUTH.READ" } }, CancellationToken.None);

        Assert.True(result.ContainsKey("auth.read"));
        Assert.True(result["AUTH.READ"]);
    }
}
