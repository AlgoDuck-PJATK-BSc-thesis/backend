namespace AlgoDuck.Modules.Auth.Commands.ApiKeys.GenerateApiKey;

public interface IGenerateApiKeyHandler
{
    Task<GenerateApiKeyResult> HandleAsync(Guid userId, GenerateApiKeyDto dto, CancellationToken cancellationToken);
}