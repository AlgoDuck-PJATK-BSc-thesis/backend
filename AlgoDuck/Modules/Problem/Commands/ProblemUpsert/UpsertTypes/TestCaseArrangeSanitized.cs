namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;


internal class TestCaseArrangeSanitized
{
    internal required string ArrangeStripped { get; init; }
    internal required Dictionary<string, string> VariableMappings { get; init; }
}

