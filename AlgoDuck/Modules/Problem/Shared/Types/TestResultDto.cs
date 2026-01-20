namespace AlgoDuck.Modules.Problem.Shared.Types;

public class TestResultDto
{
    public required Guid TestId { get; set; }
    public required bool IsTestPassed { get; set; }
}