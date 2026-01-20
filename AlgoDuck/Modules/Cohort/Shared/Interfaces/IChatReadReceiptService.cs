using AlgoDuck.Models;

namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface IChatReadReceiptService
{
    Task<int> MarkReadUpToAsync(Guid cohortId, Guid readerUserId, Message upToMessage, CancellationToken cancellationToken);
    Task<int> GetReadByCountAsync(Guid cohortId, Guid senderUserId, Message message, CancellationToken cancellationToken);
}