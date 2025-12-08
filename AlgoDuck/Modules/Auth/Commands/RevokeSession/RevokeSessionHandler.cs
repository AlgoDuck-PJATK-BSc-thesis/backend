using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Services;
using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RevokeSession;

public sealed class RevokeSessionHandler : IRevokeSessionHandler
{
    private readonly SessionService _sessionService;
    private readonly IValidator<RevokeSessionDto> _validator;

    public RevokeSessionHandler(
        SessionService sessionService,
        IValidator<RevokeSessionDto> validator)
    {
        _sessionService = sessionService;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, RevokeSessionDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new TokenException("User is not authenticated.");
        }

        await _sessionService.RevokeSessionAsync(userId, dto.SessionId, cancellationToken);
    }
}