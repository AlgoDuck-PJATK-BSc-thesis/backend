using AlgoDuck.ModelsExternal;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class UpsertProcessedDto
{
    public required string TemplateB64 { get; set; }
    public required string ProblemTitle { get; set; }
    public required string ProblemDescription { get; set; }
    public required Guid DifficultyId { get; set; }
    public required Guid CategoryId { get; set; }
    public List<ProblemTagDto> Tags { get; set; } = [];
    public List<TestCaseDto> TestCases { get; set; } = [];
    public List<TestCaseJoined> TestCaseJoins { get; set; } = [];
    public Guid RequestingUserId { get; set; }
}