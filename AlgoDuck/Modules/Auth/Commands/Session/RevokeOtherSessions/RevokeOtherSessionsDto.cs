namespace AlgoDuck.Modules.Auth.Commands.Session.RevokeOtherSessions;

public sealed class RevokeOtherSessionsDto
{
    public Guid CurrentSessionId { get; init; }
}