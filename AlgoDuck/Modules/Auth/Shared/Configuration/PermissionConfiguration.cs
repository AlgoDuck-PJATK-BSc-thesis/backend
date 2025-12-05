using AlgoDuck.Modules.Auth.Shared.Constants;

namespace AlgoDuck.Modules.Auth.Shared.Configuration;

public sealed class PermissionConfiguration
{
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> RolePermissions { get; }

    public IReadOnlyCollection<string> AllPermissions { get; }

    public PermissionConfiguration()
    {
        var rolePermissions = new Dictionary<string, IReadOnlyCollection<string>>
        {
            {
                AuthConstants.RoleAdmin,
                new[]
                {
                    PermissionConstants.ManageUsers,
                    PermissionConstants.ManageCohorts,
                    PermissionConstants.ManageProblems,
                    PermissionConstants.ManageItems,
                    PermissionConstants.ManageContests,
                    PermissionConstants.ViewAdminDashboard,
                    PermissionConstants.ViewUserStatistics,
                    PermissionConstants.ViewUserSolutions
                }
            },
            {
                AuthConstants.RoleModerator,
                new[]
                {
                    PermissionConstants.ManageUsers,
                    PermissionConstants.ManageCohorts,
                    PermissionConstants.ManageProblems,
                    PermissionConstants.ViewUserStatistics,
                    PermissionConstants.ViewUserSolutions
                }
            },
            {
                AuthConstants.RoleUser,
                new[]
                {
                    PermissionConstants.ViewProblems,
                    PermissionConstants.SolveProblems,
                    PermissionConstants.ViewOwnStatistics,
                    PermissionConstants.ViewOwnSolutions,
                    PermissionConstants.ManageOwnAccount
                }
            }
        };

        RolePermissions = rolePermissions;

        AllPermissions = rolePermissions
            .SelectMany(x => x.Value)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
    }

    public IReadOnlyCollection<string> GetPermissionsForRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return Array.Empty<string>();

        return RolePermissions.TryGetValue(role, out var permissions)
            ? permissions
            : Array.Empty<string>();
    }

    public bool RoleHasPermission(string role, string permission)
    {
        if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(permission))
            return false;

        return RolePermissions.TryGetValue(role, out var permissions) &&
               permissions.Contains(permission);
    }
}