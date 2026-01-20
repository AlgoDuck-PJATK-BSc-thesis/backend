using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IExternalAuthProvider
{
    Task<AuthUserDto?> GetUserInfoAsync(string provider, string accessToken, CancellationToken cancellationToken);
}