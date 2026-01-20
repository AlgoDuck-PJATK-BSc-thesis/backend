using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface IChatMediaStorageService
{
    Task<ChatMediaDescriptor> StoreImageAsync(
        Guid cohortId,
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken);
}