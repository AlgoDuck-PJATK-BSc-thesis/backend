using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.Login;

public sealed class LoginResult
{
    public bool TwoFactorRequired { get; set; }
    public AuthResponse? Auth { get; set; }
    public string? ChallengeId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}