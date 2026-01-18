namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class ProblemDetailsCore
{
    public required Guid ProblemId { get; set; }
    public required string ProblemName { get; set; }
    public required CategoryDto Category { get; set; }
    public required DifficultyDto Difficulty { get; set; }
    public required Guid CreatedBy { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime? LastUpdatedAt { get; set; }
    
}