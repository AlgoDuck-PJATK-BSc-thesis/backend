using WebApplication1.Modules.UserModule.DTOs;

namespace WebApplication1.Modules.UserModule.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
        Task UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken);
    }
}