namespace AlgoDuck.Modules.Auth.Commands.ApiKeys.RevokeApiKey;

public interface IRevokeApiKeyHandler
{
    Task HandleAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken);
}