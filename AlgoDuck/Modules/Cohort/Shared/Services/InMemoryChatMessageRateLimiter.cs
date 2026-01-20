using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;

namespace AlgoDuck.Modules.Cohort.Shared.Services;

public sealed class InMemoryChatMessageRateLimiter : IChatMessageRateLimiter
{
    readonly ConcurrentDictionary<string, TokenBucketRateLimiter> _limiters = new();

    readonly TokenBucketRateLimiterOptions _options = new()
    {
        TokenLimit = 5,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0,
        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
        TokensPerPeriod = 5,
        AutoReplenishment = true
    };

    public Task CheckAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken)
    {
        var key = $"chat:{cohortId:N}:{userId:N}";
        var limiter = _limiters.GetOrAdd(key, _ => new TokenBucketRateLimiter(_options));

        var lease = limiter.AttemptAcquire();
        if (!lease.IsAcquired)
        {
            lease.Dispose();
            throw new RateLimitExceededException("Too many messages. Please slow down.");
        }

        lease.Dispose();
        return Task.CompletedTask;
    }
}