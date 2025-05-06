using WebApplication1.Modules.AuthModule.DTOs;

namespace WebApplication1.Modules.AuthModule.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto, CancellationToken cancellationToken);
    Task LoginAsync(LoginDto dto, HttpResponse response, CancellationToken cancellationToken);
    Task RefreshTokenAsync(RefreshDto dto, HttpResponse response, CancellationToken cancellationToken);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken);
}
