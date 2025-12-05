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
        var userId = GetUserId();
        await _validator.ValidateAndThrowAsync(userId, cancellationToken);

        var currentSessionId = GetCurrentSessionId();
        var sessions = await _handler.HandleAsync(userId, currentSessionId, cancellationToken);
        return Ok(sessions);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(userIdClaim.Value, out var id) ? id : Guid.Empty;
    }

    private Guid GetCurrentSessionId()
    {
        var sessionClaim = User.FindFirst("session_id");
        if (sessionClaim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(sessionClaim.Value, out var id) ? id : Guid.Empty;
    }
}