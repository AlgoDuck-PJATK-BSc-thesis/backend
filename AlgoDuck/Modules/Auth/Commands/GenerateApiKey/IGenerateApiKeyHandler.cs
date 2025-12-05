using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.GenerateApiKey;

public sealed class GenerateApiKeyResult
{
    public ApiKeyDto ApiKey { get; set; } = null!;
    public string RawKey { get; set; } = string.Empty;
}

public interface IGenerateApiKeyHandler
{
    Task<GenerateApiKeyResult> HandleAsync(Guid userId, GenerateApiKeyDto dto, CancellationToken cancellationToken);
}