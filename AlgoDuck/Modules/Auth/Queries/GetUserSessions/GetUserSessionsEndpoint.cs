using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Queries.GetUserSessions;

[ApiController]
[Route("api/auth/sessions")]
public sealed class GetUserSessionsEndpoint : ControllerBase
{
    private readonly IGetUserSessionsHandler _handler;
    private readonly IValidator<Guid> _validator;

    public GetUserSessionsEndpoint(
        IGetUserSessionsHandler handler,
        IValidator<Guid> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<UserSessionDto>>> GetUserSessions(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        await _validator.ValidateAndThrowAsync(userId, cancellationToken);

        var currentSessionId = GetSessionIdFromClaims();
        var sessions = await _handler.HandleAsync(userId, currentSessionId, cancellationToken);
        return Ok(sessions);
    }

    private Guid GetUserIdFromClaims()
    {
        var subjectClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (subjectClaim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(subjectClaim.Value, out var id) ? id : Guid.Empty;
    }

    private Guid GetSessionIdFromClaims()
    {
        var sessionClaim = User.FindFirst("session_id");
        if (sessionClaim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(sessionClaim.Value, out var id) ? id : Guid.Empty;
    }
}