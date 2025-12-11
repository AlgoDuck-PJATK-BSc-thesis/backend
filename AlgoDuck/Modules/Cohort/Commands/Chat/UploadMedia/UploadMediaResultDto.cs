namespace AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;

public sealed class UploadMediaResultDto
{
    public Guid CohortId { get; init; }
    public Guid UserId { get; init; }
    public string MediaKey { get; init; } = string.Empty;
    public string MediaUrl { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
}