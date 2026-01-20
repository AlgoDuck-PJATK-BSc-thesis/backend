
namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class JobData<T>
{
    public required Guid ProblemId { get; set; }
    public required Guid CommissioningUserId { get; set; }
    public List<ValidationResponse> CachedResponses { get; set; } = [];
    public required UpsertJobType UpsertJobType { get; set; }
    public T? JobBody { get; set; } = default;
}