using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface IChatModerationService
{
    Task<ChatModerationResult> CheckMessageAsync(
        Guid userId,
        Guid cohortId,
        string content,
        CancellationToken cancellationToken);
}