using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodeTopLevelStat
{
    public TopLevelStatement TopLevelStatement { get; set; }
    public string? Uri { get; set; }
}