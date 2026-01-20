namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class LoginFlowResponseDto
{
    public string Message { get; init; } = string.Empty;
    public bool TwoFactorRequired { get; init; }
    public AuthResponse? Auth { get; init; }
    public string? ChallengeId { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}