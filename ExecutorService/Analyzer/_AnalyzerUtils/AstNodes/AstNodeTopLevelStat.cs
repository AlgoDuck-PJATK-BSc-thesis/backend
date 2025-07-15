using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes;

public class AstNodeTopLevelStat
{
    public TopLevelStatement TopLevelStatement { get; set; }
    public string? Uri { get; set; }
}