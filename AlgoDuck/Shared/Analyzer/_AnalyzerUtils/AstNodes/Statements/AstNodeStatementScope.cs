using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;

public class AstNodeStatementScope
{
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }
    public required Scope OwnScope { get; set; }
    public List<AstNodeStatement> ScopedStatements { get; set; } = [];
}