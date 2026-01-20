namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface IChatMessageRateLimiter
{
    Task CheckAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken);
}