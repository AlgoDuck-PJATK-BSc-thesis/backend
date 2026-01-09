namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.LoadProblemCreateFormStateFromProblem;

public class FullProblemDataDto
{
    public required string ProblemName { get; set; }

    public required Guid CategoryId { get; set; }
    public required Guid DifficultyId { get; set; }
    public required ICollection<TagDto> Tags { get; set; }
}

public class TagDto
{
    public required string TagName { get; set; }
}