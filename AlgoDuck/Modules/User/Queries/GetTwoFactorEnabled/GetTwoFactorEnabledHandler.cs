using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetTwoFactorEnabled;

public sealed class GetTwoFactorEnabledHandler : IGetTwoFactorEnabledHandler
{
    private readonly IUserRepository _userRepository;

    public GetTwoFactorEnabledHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<TwoFactorStatusDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("User identifier is invalid.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        return new TwoFactorStatusDto
        {
            UserId = user.Id,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
    }
}