namespace AlgoDuck.Modules.Cohort.Commands.User.Chat.UploadMedia;

public interface IUploadMediaHandler
{
    Task<UploadMediaResultDto> HandleAsync(Guid userId, UploadMediaDto dto, CancellationToken cancellationToken);
}