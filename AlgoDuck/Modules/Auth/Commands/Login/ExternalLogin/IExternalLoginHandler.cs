using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.Login.ExternalLogin;

public interface IExternalLoginHandler
{
    Task<AuthResponse> HandleAsync(ExternalLoginDto dto, CancellationToken cancellationToken);
}