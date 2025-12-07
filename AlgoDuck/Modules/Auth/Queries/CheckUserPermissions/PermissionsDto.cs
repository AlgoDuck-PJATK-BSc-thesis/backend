namespace AlgoDuck.Modules.Auth.Queries.CheckUserPermissions;

public sealed class PermissionsDto
{
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}