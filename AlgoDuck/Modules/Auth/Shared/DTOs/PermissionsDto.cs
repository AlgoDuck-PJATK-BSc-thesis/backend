namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class PermissionsDto
{
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
}