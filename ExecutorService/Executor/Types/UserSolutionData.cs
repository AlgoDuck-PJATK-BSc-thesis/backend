using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;

namespace ExecutorService.Executor.Types;

public class UserSolutionData
{
    public Guid ExecutionId { get; } = Guid.NewGuid();
    public Guid SigningKey { get; } = Guid.NewGuid(); // maybe could just use ExecutionId? Edit: Could be, but I'll keep it separate to mitigate the human risk of someone confusing the 2
    public required StringBuilder FileContents { get; init; }
    public required string Lang { get; set; }
    public required ExecutionStyle ExecutionStyle { get; init; }
    public Guid? ExerciseId { get; init; }
    public string MainClassName { get; set; } = string.Empty;
    public bool PassedValidation { get; set; } = false;
    public MainMethod? MainMethod { get; set; }

    public void IngestCodeAnalysisResult(CodeAnalysisResult codeAnalysisResult)
    {
        MainClassName = codeAnalysisResult.MainClassName;
        MainMethod = codeAnalysisResult.MainMethodIndices;
        PassedValidation = codeAnalysisResult.PassedValidation;
    }
}