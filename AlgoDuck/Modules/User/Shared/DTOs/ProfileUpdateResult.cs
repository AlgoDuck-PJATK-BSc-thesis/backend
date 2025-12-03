namespace AlgoDuck.Modules.User.Shared.DTOs;

public sealed class ProfileUpdateResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}