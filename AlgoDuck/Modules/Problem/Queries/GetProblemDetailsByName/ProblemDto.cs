namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;

public class ProblemDto
{
    public required Guid ProblemId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required CategoryDto Category { get; init; }
    public required DifficultyDto Difficulty { get; init; }
    public required string TemplateContents { get; set; }
    public IEnumerable<TestCaseDto> TestCases { get; set; } = [];
}

public class CategoryDto
{
    public required string Name { get; init; }
}

public class DifficultyDto
{
    public required string Name { get; init; }
}

public class TestCaseDto
{
    public required Guid TestCaseId { get; set; }
    public required string Display { get; set; }
    public required string DisplayRes { get; set; }
    public required bool IsPublic { get; set; }
    
}