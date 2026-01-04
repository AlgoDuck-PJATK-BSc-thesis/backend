namespace AlgoDuck.Modules.Cohort.Commands.User.Chat.UploadMedia;

public sealed class UploadMediaDto
{
    public Guid CohortId { get; init; }
    public IFormFile File { get; init; } = default!;
}