namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public sealed class ChatMediaDescriptor
{
    public string Key { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public ChatMediaType MediaType { get; init; }
}