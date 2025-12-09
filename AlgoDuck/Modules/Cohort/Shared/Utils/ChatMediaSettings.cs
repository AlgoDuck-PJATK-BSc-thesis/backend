namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public sealed class ChatMediaSettings
{
    public long MaxFileSizeBytes { get; init; } = 64L * 1024L * 1024L;
    public string[] AllowedContentTypes { get; init; } = { "image/jpeg", "image/png" };
    public string RootPrefix { get; init; } = "cohort-chat";
}