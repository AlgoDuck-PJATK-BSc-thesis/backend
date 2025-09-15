using AlgoDuck.Modules.User.DTOs;

namespace AlgoDuck.Modules.User.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
        Task UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken);
    }
}