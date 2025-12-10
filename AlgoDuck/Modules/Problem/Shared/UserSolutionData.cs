using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

namespace AlgoDuck.Modules.Problem.Shared;

public class UserSolutionData
{
    public Guid SigningKey = Guid.NewGuid();
    public Guid ExecutionId = Guid.NewGuid();
    public StringBuilder FileContents { get; init; } = new();
    public string MainClassName { get; set; } = string.Empty;
    public bool PassedValidation { get; set; } = false;
    public MainMethod? MainMethod { get; set; }

    public void IngestCodeAnalysisResult(CodeAnalysisResult codeAnalysisResult)
    {
        MainClassName = codeAnalysisResult.MainClassName;
        MainMethod = codeAnalysisResult.MainMethodIndices;
        PassedValidation = codeAnalysisResult.PassedValidation;
    }

    public Dictionary<string, string> GetFileContents()
    {
        
        
        return new Dictionary<string, string>()
        {
            [MainClassName] = Convert.ToBase64String(Encoding.UTF8.GetBytes(FileContents.ToString()))
        };
    }
}