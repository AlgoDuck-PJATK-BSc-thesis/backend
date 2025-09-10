using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;

namespace ExecutorService.Executor.Types;

public class UserSolutionData(string lang, StringBuilder fileContents, string? exerciseId)
{
    public Guid ExecutionId { get; } = Guid.NewGuid();

    public Guid SigningKey { get; } = Guid.NewGuid(); // maybe could just use ExecutionId? Edit: Could be, but I'll keep it separate to mitigate the human risk of someone confusing the 2
    public Guid ExerciseId { get; } = exerciseId != null ? Guid.Parse(exerciseId) : Guid.Empty;
    public string Lang => lang;

    public StringBuilder FileContents => fileContents;

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