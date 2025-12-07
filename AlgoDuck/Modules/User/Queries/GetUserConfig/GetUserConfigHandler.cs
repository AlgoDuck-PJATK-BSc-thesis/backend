using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserConfig;

public sealed class GetUserConfigHandler : IGetUserConfigHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserConfigHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserConfigDto> HandleAsync(GetUserConfigRequestDto query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(query.UserId);
        }

        var config = user.UserConfig;

        return new UserConfigDto
        {
            IsDarkMode = config?.IsDarkMode ?? false,
            IsHighContrast = config?.IsHighContrast ?? false,
            Language = config?.Language ?? string.Empty,
            S3AvatarUrl = string.Empty
        };
    }
}