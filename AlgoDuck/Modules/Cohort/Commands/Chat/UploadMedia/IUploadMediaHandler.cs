namespace AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;

public interface IUploadMediaHandler
{
    Task<UploadMediaResultDto> HandleAsync(Guid userId, UploadMediaDto dto, CancellationToken cancellationToken);
}