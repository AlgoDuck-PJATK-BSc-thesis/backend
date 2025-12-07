using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Queries.GetCurrentUser;

public interface IGetCurrentUserHandler
{
    Task<AuthUserDto?> HandleAsync(Guid userId, CancellationToken cancellationToken);
}