using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class DevExternalAuthProvider : IExternalAuthProvider
{
    public Task<AuthUserDto?> GetUserInfoAsync(string provider, string accessToken, CancellationToken cancellationToken)
    {
        return Task.FromResult<AuthUserDto?>(null);
    }
}