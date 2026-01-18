namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class DifficultyDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal RewardScaler { get; set; }
}