namespace AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;

public sealed class UserConfigDto
{
    public bool IsDarkMode { get; init; }
    public bool IsHighContrast { get; init; }
    public string Language { get; init; } = string.Empty;
    public string S3AvatarUrl { get; init; } = string.Empty;
}