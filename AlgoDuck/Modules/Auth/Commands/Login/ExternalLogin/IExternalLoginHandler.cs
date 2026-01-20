using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.Login.ExternalLogin;

public interface IExternalLoginHandler
{
    Task<LoginFlowResponseDto> HandleAsync(ExternalLoginDto dto, CancellationToken cancellationToken);
}