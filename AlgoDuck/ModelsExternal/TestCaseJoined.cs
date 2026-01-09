namespace AlgoDuck.ModelsExternal;

public class TestCaseJoined
{
    public required Guid TestCaseId { get; set; }
    public required string CallFunc { get; set; }
    public required bool IsPublic { get; set; }
    public required Guid ProblemProblemId { get; set; }
    public required string Display { get; set; }
    public required string DisplayRes { get; set; }
    public required int VariableCount { get; set; } /* Technically not necessary but helps us create more deterministic variable substitution keys */
    public required string Setup { get; set; } = string.Empty; // Arrange
    public required string[] Call { get; set; } = []; // Act
    public required string Expected { get; set; } = string.Empty; // Assert
    public bool OrderMatters = false; /* TODO: Make this not static*/
}