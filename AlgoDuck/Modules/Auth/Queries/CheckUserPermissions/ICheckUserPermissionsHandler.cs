namespace AlgoDuck.Modules.Auth.Queries.CheckUserPermissions;

public interface ICheckUserPermissionsHandler
{
    Task<IDictionary<string, bool>> HandleAsync(Guid userId, PermissionsDto dto, CancellationToken cancellationToken);
}