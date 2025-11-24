namespace AlgoDuck.ModelsExternal;

public class TestCaseJoined
{
    public required Guid TestCaseId { get; set; }
    public required string CallFunc { get; set; }
    public required bool IsPublic { get; set; }
    public required Guid ProblemProblemId { get; set; }
    public required string Display { get; set; }
    public required string DisplayRes { get; set; }
    public required string Setup { get; set; } = string.Empty; // Arrange
    public required string[] Call { get; set; } = []; // Act
    public required string Expected { get; set; } = string.Empty; // Assert
}