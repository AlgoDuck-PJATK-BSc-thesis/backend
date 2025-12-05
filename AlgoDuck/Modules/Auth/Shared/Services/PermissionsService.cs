using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class PermissionsService : IPermissionsService
{
    private readonly IPermissionsRepository _permissionsRepository;

    public PermissionsService(IPermissionsRepository permissionsRepository)
    {
        _permissionsRepository = permissionsRepository;
    }

    public async Task EnsureUserHasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new PermissionException("User identifier is invalid.");
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new PermissionException("Permission name is required.");
        }

        var permissions = await _permissionsRepository.GetUserPermissionsAsync(userId, cancellationToken);

        if (!PermissionChecker.HasAnyPermission(permissions, permission))
        {
            throw new PermissionException("User does not have the required permission.");
        }
    }
}