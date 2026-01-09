namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class MethodRecommendation
{
    public required string MethodName { get; set; }
    public required string QualifiedName { get; set; }
    public List<FunctionParam> FunctionParams { get; set; } = [];
    public List<string> Generics { get; set; } = [];
    public List<string> Modifiers { get; set; } = [];
    public required string AccessModifier { get; set; }
    public required string ReturnType { get; set; }
}