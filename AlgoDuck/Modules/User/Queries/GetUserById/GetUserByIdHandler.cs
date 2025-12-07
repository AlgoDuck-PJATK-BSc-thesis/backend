using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserById;

public sealed class GetUserByIdHandler : IGetUserByIdHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> HandleAsync(GetUserByIdRequestDto requestDto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(requestDto.UserId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException("User not found.");
        }

        var config = user.UserConfig;

        return new UserDto
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Coins = user.Coins,
            Experience = user.Experience,
            AmountSolved = user.AmountSolved,
            CohortId = user.CohortId,
            Language = config?.Language ?? string.Empty,
            S3AvatarUrl = string.Empty
        };
    }
}