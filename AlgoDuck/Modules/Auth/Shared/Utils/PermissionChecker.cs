namespace AlgoDuck.Modules.Auth.Shared.Utils;

public static class PermissionChecker
{
    public static bool HasAnyPermission(IEnumerable<string> userPermissions, params string[] requiredPermissions)
    {
        var set = new HashSet<string>(userPermissions);
        foreach (var required in requiredPermissions)
        {
            if (set.Contains(required))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAllPermissions(IEnumerable<string> userPermissions, params string[] requiredPermissions)
    {
        var set = new HashSet<string>(userPermissions);
        foreach (var required in requiredPermissions)
        {
            if (!set.Contains(required))
            {
                return false;
            }
        }

        return true;
    }
}