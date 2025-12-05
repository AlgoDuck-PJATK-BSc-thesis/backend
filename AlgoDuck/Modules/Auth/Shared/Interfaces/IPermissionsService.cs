namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IPermissionsService
{
    Task EnsureUserHasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken);
}