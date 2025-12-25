using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeTypeScope<T> where T : BaseType<T>
{
    public T? OwnerMember { get; set; }
    public List<AstNodeTypeMember<T>> TypeMembers { get; } = [];
    public required Scope OwnScope { get; set; }
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }

}