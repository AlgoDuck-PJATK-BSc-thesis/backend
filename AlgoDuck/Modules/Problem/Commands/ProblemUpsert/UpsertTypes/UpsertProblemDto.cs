using System.ComponentModel.DataAnnotations;
using AlgoDuck.ModelsExternal;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class UpsertProblemDto
{
    public required string TemplateB64 { get; set; }
    public required string ProblemTitle { get; set; }
    public required string ProblemDescription { get; set; }
    public required Guid DifficultyId { get; set; }
    public required Guid CategoryId { get; set; }
    public List<ProblemTagDto> Tags { get; set; } = [];
    public List<TestCaseDto> TestCases { get; set; } = [];
    internal List<TestCaseJoined> TestCaseJoins { get; set; } = [];
    internal Guid RequestingUserId { get; set; }
}