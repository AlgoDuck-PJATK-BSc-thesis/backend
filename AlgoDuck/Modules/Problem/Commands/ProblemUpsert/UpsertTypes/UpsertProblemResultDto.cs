namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class UpsertProblemResultDto
{
    public required Guid JobId { get; set; }
    public required Guid ProblemId { get; set; }
}