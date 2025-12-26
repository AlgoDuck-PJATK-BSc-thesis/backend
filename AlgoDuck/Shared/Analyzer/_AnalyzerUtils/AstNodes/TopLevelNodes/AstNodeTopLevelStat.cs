using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodeTopLevelStat
{
    public TopLevelStatement TopLevelStatement { get; set; }
    public string? Uri { get; set; }
}