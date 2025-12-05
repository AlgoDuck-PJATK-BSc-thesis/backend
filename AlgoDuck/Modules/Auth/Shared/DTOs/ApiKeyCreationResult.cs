namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class ApiKeyCreationResult
{
    public ApiKeyDto ApiKey { get; init; } = null!;
    public string RawKey { get; init; } = string.Empty;
}