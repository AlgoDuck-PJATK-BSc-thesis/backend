using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Queries.GetApiKeys;

public interface IGetApiKeysHandler
{
    Task<IReadOnlyList<ApiKeyDto>> HandleAsync(Guid userId, CancellationToken cancellationToken);
}