namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public class AssistantRequestDto
{
    public required Guid ChatId { get; set; }
    public required Guid ExerciseId { get; set; }
    public required string CodeB64 { get; set; }
    public required string Query { get; set; }
    internal Guid UserId { get; set; }
}