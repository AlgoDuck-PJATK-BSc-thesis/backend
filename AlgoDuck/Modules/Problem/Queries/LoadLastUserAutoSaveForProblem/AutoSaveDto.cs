namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

public class AutoSaveDto
{
    public required Guid ProblemId { get; set; }
    public required string UserCodeB64 { get; init; }
}

public class AutoSaveResponseDto
{
    public required Guid ProblemId { get; set; }
    public required string UserCodeB64 { get; init; }
    public ICollection<TestResults> TestResults { get; init; } = [];
    
}

public class TestResults
{
    public required Guid TestId { get; init; }
    public required bool IsPassed { get; init; }
}

public class AutoSaveRequestDto
{
    public required Guid ProblemId { get; set; }
    internal Guid UserId { get; set; }
}