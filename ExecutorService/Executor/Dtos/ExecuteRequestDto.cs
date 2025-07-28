namespace ExecutorService.Executor.Dtos;

public class ExecuteRequestDto
{
    public string CodeB64 { get; set; } = string.Empty;
    public string Lang { get; set; } = string.Empty;
    public string ExerciseId { get; set; } = string.Empty;
}