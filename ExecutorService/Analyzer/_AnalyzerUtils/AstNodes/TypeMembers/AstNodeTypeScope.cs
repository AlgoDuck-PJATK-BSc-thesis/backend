using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeTypeScope<T> where T : IType<T>
{
    public T? OwnerMember { get; set; }
    public List<AstNodeTypeMember<T>> TypeMembers { get; } = [];
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }

}