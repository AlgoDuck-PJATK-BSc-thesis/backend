namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class PermissionCheckResult
{
    public Guid UserId { get; init; }
    public IReadOnlyList<string> RequestedPermissions { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> GrantedPermissions { get; init; } = Array.Empty<string>();
    public bool AllGranted { get; init; }
}