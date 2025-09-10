using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodeProgram
{
    public List<AstNodeCompilationUnit> ProgramCompilationUnits { get; set; } = []; 
    
}