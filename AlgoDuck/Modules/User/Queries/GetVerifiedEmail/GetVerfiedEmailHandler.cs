using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetVerifiedEmail;

public sealed class GetVerifiedEmailHandler : IGetVerifiedEmailHandler
{
    private readonly IUserRepository _userRepository;

    public GetVerifiedEmailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetVerifiedEmailResultDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
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

        return new GetVerifiedEmailResultDto
        {
            UserId = user.Id,
            EmailConfirmed = user.EmailConfirmed
        };
    }
}