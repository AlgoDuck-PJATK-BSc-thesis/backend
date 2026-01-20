using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

public abstract class BaseType<T> where T : BaseType<T>
{
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Default;
    public Token Name { get; set; }
    public AstNodeTypeScope<T>? TypeScope { get; set; }
    // public List<AstNodeTypeMember<BaseType>> Members { get; set; } = [];
}