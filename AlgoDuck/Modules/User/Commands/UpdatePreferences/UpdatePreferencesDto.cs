namespace AlgoDuck.Modules.User.Commands.UpdatePreferences;

public sealed class UpdatePreferencesDto
{
    public bool IsDarkMode { get; set; }
    public bool IsHighContrast { get; set; }
    public string Language { get; set; } = "en";
    public bool EmailNotificationsEnabled { get; set; }
    public bool PushNotificationsEnabled { get; set; }
}