using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Mappers;

public static class UserMapper
{
    public static AuthUserDto ToAuthUserDto(ApplicationUser user)
    {
        return new AuthUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed
        };
    }
}