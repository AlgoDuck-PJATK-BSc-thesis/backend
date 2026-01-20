using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.Login.Register;

public interface IRegisterHandler
{
    Task<AuthUserDto> HandleAsync(RegisterDto dto, CancellationToken cancellationToken);
}