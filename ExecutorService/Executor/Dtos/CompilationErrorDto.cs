namespace ExecutorService.Executor.Dtos;

public class CompilationErrorDto(string errorMsg)
{
    public string ErrorMsg { get; init; } = errorMsg;
}