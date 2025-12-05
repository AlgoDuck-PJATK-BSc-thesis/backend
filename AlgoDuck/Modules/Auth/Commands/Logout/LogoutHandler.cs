using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Services;
using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.Logout;

public sealed class LogoutHandler : ILogoutHandler
{
    private readonly SessionService _sessionService;
    private readonly IValidator<LogoutDto> _validator;

    public LogoutHandler(SessionService sessionService, IValidator<LogoutDto> validator)
    {
        _sessionService = sessionService;
        _validator = validator;
    }

    public async Task HandleAsync(LogoutDto dto, Guid? currentUserId, Guid? currentSessionId, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (currentUserId is null || currentUserId == Guid.Empty)
        {
            throw new PermissionException("User is not authenticated.");
        }

        Guid sessionIdToRevoke;

        if (dto.SessionId.HasValue && dto.SessionId.Value != Guid.Empty)
        {
            sessionIdToRevoke = dto.SessionId.Value;
        }
        else
        {
            if (currentSessionId is null || currentSessionId == Guid.Empty)
            {
                throw new TokenException("Session identifier is missing.");
            }

            sessionIdToRevoke = currentSessionId.Value;
        }

        await _sessionService.RevokeSessionAsync(currentUserId.Value, sessionIdToRevoke, cancellationToken);
    }
}