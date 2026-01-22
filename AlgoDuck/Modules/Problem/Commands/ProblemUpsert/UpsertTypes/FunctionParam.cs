using System.ComponentModel.DataAnnotations;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class FunctionParam
{
    public required string Type { get; set; }
    
    public required string Name { get; set; }
}