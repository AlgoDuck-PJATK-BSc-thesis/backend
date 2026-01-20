using System.Security.Claims;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IPermissionsRepository
{
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Claim>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken);
}