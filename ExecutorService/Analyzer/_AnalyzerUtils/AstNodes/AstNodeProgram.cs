using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes;

public class AstNodeProgram
{
    public List<OneOf<AstNodeClass, AstNodeTopLevelStat>> ProgramClasses { get; set; } = new(); //TODO change this, confusing naming
}