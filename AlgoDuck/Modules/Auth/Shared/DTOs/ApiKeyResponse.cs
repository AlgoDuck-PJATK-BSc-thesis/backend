namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class ApiKeyResponse
{
    public ApiKeyDto ApiKey { get; init; } = new ApiKeyDto();
    public string PlainTextKey { get; init; } = string.Empty;
}