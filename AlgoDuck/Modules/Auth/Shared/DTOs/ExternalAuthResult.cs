namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class ExternalAuthResult
{
    public AuthResponse? Auth { get; init; }
    public bool RequiresRegistration { get; init; }
    public string? Provider { get; init; }
    public string? ProviderUserId { get; init; }
    public string? Email { get; init; }
}