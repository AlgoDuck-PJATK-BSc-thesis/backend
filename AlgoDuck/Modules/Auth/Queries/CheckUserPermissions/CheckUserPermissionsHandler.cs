using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Queries.CheckUserPermissions;

public sealed class CheckUserPermissionsHandler : ICheckUserPermissionsHandler
{
    private readonly IPermissionsService _permissionsService;

    public CheckUserPermissionsHandler(IPermissionsService permissionsService)
    {
        _permissionsService = permissionsService;
    }

    public async Task<IDictionary<string, bool>> HandleAsync(Guid userId, PermissionsDto dto, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new PermissionException("User identifier is invalid.");
        }

        var permissions = dto.Permissions;
        if (permissions.Count == 0)
        {
            return new Dictionary<string, bool>();
        }

        var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var permission in permissions)
        {
            if (string.IsNullOrWhiteSpace(permission))
            {
                continue;
            }

            var hasPermission = await HasPermissionAsync(userId, permission, cancellationToken);
            result[permission] = hasPermission;
        }

        return result;
    }

    private async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken)
    {
        try
        {
            await _permissionsService.EnsureUserHasPermissionAsync(userId, permission, cancellationToken);
            return true;
        }
        catch (PermissionException)
        {
            return false;
        }
    }
}