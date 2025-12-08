namespace AlgoDuck.Modules.Auth.Commands.RevokeOtherSessions;

public sealed class RevokeOtherSessionsDto
{
    public Guid CurrentSessionId { get; init; }
}