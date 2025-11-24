using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeTypeScope<T> where T : IType<T>
{
    public T? OwnerMember { get; set; }
    public List<AstNodeTypeMember<T>> TypeMembers { get; } = [];
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }

}