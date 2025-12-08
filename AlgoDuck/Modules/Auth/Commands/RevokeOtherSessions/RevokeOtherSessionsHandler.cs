using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Services;
using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RevokeOtherSessions;

public sealed class RevokeOtherSessionsHandler : IRevokeOtherSessionsHandler
{
    private readonly SessionService _sessionService;
    private readonly IValidator<RevokeOtherSessionsDto> _validator;

    public RevokeOtherSessionsHandler(
        SessionService sessionService,
        IValidator<RevokeOtherSessionsDto> validator)
    {
        _sessionService = sessionService;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, RevokeOtherSessionsDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new TokenException("User is not authenticated.");
        }

        await _sessionService.RevokeOtherSessionsAsync(userId, dto.CurrentSessionId, cancellationToken);
    }
}