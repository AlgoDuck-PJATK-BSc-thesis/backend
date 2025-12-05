using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;

public interface IVerifyTwoFactorLoginHandler
{
    Task<AuthResponse> HandleAsync(VerifyTwoFactorLoginDto dto, CancellationToken cancellationToken);
}