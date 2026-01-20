using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;

public sealed class UpdatePreferencesDto
{
    public bool IsDarkMode { get; set; }
    public bool IsHighContrast { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public List<Reminder>? WeeklyReminders { get; set; }
}
