namespace AlgoDuck.Modules.Problem.DTOs.ProblemDtos;

public class ProblemDto(Guid problemId, string name, string description,  DifficultyDto difficultyDto, CategoryDto categoryDto, TypeDto typeDto)
{
    public Guid ProblemId { get; init; } = problemId;
    public string Title { get; init; } = name;
    public string Description { get; init; } = description;
    public CategoryDto Category { get; init; } = categoryDto;
    public DifficultyDto Difficulty { get; init; } = difficultyDto;
    public TypeDto Type { get; init; } =  typeDto;
    public string? TemplateContents { get; set; } = string.Empty;
    public List<TestCaseDto>? TestCases { get; set; } = [];
}