namespace WebApplication1.Modules.ProblemModule.DTOs.ExecutorDtos;

public class TestResultDto
{
    public string TestId { get; set; } = string.Empty;
    public bool IsTestPassed { get; set; } = false;
}