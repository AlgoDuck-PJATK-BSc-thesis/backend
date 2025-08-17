namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeCLassScope
{
    public AstNodeClass? OwnerClass { get; set; }
    public List<AstNodeClassMember> ClassMembers { get; } = new List<AstNodeClassMember>();
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }
}