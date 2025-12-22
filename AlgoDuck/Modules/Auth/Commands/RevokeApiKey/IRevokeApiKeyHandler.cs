namespace AlgoDuck.Modules.Auth.Commands.RevokeApiKey;

public interface IRevokeApiKeyHandler
{
    Task HandleAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken);
}