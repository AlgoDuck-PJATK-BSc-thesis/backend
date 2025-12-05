using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Queries.GetUserByToken;

public interface IGetUserByTokenHandler
{
    Task<AuthUserDto?> HandleAsync(UserByTokenDto dto, CancellationToken cancellationToken);
}