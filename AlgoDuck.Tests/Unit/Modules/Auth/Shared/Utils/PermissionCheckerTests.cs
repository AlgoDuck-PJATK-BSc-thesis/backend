using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Utils;

public sealed class PermissionCheckerTests
{
    [Fact]
    public void HasAnyPermission_WhenAnyMatches_ReturnsTrue()
    {
        var userPermissions = new[] { "auth.read", "auth.manage", "user.profile" };

        var result = PermissionChecker.HasAnyPermission(userPermissions, "auth.manage", "other");

        Assert.True(result);
    }

    [Fact]
    public void HasAnyPermission_WhenNoneMatch_ReturnsFalse()
    {
        var userPermissions = new[] { "auth.read", "user.profile" };

        var result = PermissionChecker.HasAnyPermission(userPermissions, "auth.manage", "other");

        Assert.False(result);
    }

    [Fact]
    public void HasAnyPermission_WhenRequiredEmpty_ReturnsFalse()
    {
        var userPermissions = new[] { "auth.read" };

        var result = PermissionChecker.HasAnyPermission(userPermissions);

        Assert.False(result);
    }

    [Fact]
    public void HasAllPermissions_WhenAllPresent_ReturnsTrue()
    {
        var userPermissions = new[] { "auth.read", "auth.manage", "user.profile" };

        var result = PermissionChecker.HasAllPermissions(userPermissions, "auth.read", "auth.manage");

        Assert.True(result);
    }

    [Fact]
    public void HasAllPermissions_WhenAnyMissing_ReturnsFalse()
    {
        var userPermissions = new[] { "auth.read", "user.profile" };

        var result = PermissionChecker.HasAllPermissions(userPermissions, "auth.read", "auth.manage");

        Assert.False(result);
    }

    [Fact]
    public void HasAllPermissions_WhenRequiredEmpty_ReturnsTrue()
    {
        var userPermissions = new[] { "auth.read" };

        var result = PermissionChecker.HasAllPermissions(userPermissions);

        Assert.True(result);
    }
}