using AlgoDuck.Models;

namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface IChatMessageRepository
{
    Task<Message> AddAsync(Message message, CancellationToken cancellationToken);

    Task<IReadOnlyList<Message>> GetPagedForCohortAsync(
        Guid cohortId,
        DateTime? beforeCreatedAtUtc,
        int pageSize,
        CancellationToken cancellationToken);

    Task<bool> SoftDeleteAsync(Guid messageId, Guid requesterId, CancellationToken cancellationToken);
}