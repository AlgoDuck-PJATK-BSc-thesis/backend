namespace AlgoDuck.Modules.Auth.Queries.Permissions.CheckUserPermissions;

public interface ICheckUserPermissionsHandler
{
    Task<IDictionary<string, bool>> HandleAsync(Guid userId, PermissionsDto dto, CancellationToken cancellationToken);
}