using System.Collections.Concurrent;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;

namespace AlgoDuck.Modules.Cohort.Shared.Services;

public sealed class ChatReadReceiptService : IChatReadReceiptService
{
    private readonly ConcurrentDictionary<(Guid CohortId, Guid UserId), DateTime> _lastReadAtUtc = new();

    public Task<int> MarkReadUpToAsync(Guid cohortId, Guid readerUserId, Message upToMessage, CancellationToken cancellationToken)
    {
        var readAtUtc = upToMessage.CreatedAt;
        var key = (cohortId, readerUserId);

        _lastReadAtUtc.AddOrUpdate(
            key,
            readAtUtc,
            (_, existing) => existing >= readAtUtc ? existing : readAtUtc
        );

        var count = GetReadByCountInternal(cohortId, upToMessage.UserId, upToMessage.CreatedAt);
        return Task.FromResult(count);
    }

    public Task<int> GetReadByCountAsync(Guid cohortId, Guid senderUserId, Message message, CancellationToken cancellationToken)
    {
        var count = GetReadByCountInternal(cohortId, senderUserId, message.CreatedAt);
        return Task.FromResult(count);
    }

    private int GetReadByCountInternal(Guid cohortId, Guid senderUserId, DateTime messageCreatedAtUtc)
    {
        var count = 0;

        foreach (var kvp in _lastReadAtUtc)
        {
            if (kvp.Key.CohortId != cohortId) continue;
            if (kvp.Key.UserId == senderUserId) continue;
            if (kvp.Value >= messageCreatedAtUtc) count++;
        }

        return count;
    }
}
