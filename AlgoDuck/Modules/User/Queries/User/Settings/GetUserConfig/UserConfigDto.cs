
using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;

public sealed class UserConfigDto
{
    public bool IsDarkMode { get; init; }
    public bool IsHighContrast { get; init; }
    public bool EmailNotificationsEnabled { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<Reminder> WeeklyReminders { get; init; } = new();
    public string S3AvatarUrl { get; init; } = string.Empty;
}