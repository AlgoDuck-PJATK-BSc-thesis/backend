namespace AlgoDuck.Modules.Problem.Queries.QueryAssistant;

public class AssistantRequestDto
{
    public Guid ExerciseId { get; set; }
    public string DuckName { get; set; } = string.Empty;
    public string UserCodeB64 { get; set; } = string.Empty;
}