using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.ApiKeys.GenerateApiKey;

public sealed class GenerateApiKeyResult
{
    public ApiKeyDto ApiKey { get; set; } = null!;
    public string RawKey { get; set; } = string.Empty;
}