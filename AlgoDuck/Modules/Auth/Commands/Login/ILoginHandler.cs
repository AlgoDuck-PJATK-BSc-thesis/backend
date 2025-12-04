using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.Login;

public interface ILoginHandler
{
    Task<AuthResponse> HandleAsync(LoginDto dto, CancellationToken cancellationToken);
}