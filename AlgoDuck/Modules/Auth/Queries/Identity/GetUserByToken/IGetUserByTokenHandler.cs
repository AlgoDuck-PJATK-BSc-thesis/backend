using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Queries.Identity.GetUserByToken;

public interface IGetUserByTokenHandler
{
    Task<AuthUserDto?> HandleAsync(UserByTokenDto dto, CancellationToken cancellationToken);
}