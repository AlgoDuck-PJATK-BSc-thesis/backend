
using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class TestCaseDto
{
    public Guid? TestCaseId { get; set; }
    public required string Display { get; set; }
    public required string DisplayRes {get; set; }
    public required string ArrangeB64 { get; set; }
    public required MethodRecommendation CallMethod { get; set; }
    public List<FunctionParam> CallArgs { get; set; } = [];
    public required FunctionParam Expected { get; set; }
    public required bool IsPublic { get; set; }
    public required bool OrderMatters { get; set; }
    public required bool InPlace { get; set; }
    
    internal string? ResolvedFunctionCall { get; set; }
}
