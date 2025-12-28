namespace AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;

public class ProblemDisplayDto
{
    public required Guid ProblemId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DifficultyDto Difficulty { get; set; }
    public required ICollection<TagDto> Tags { get; set; }
}

public class TagDto
{
    public required string Name { get; set; }
}

public class DifficultyDto
{
    public required string Name { get; set; }
}