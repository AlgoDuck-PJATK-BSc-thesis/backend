using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.Register;

public interface IRegisterHandler
{
    Task<AuthUserDto> HandleAsync(RegisterDto dto, CancellationToken cancellationToken);
}