namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;

public class AstNodeStatementScope
{
    public int ScopeBeginOffset { get; set; }
    public int ScopeEndOffset { get; set; }
    public List<AstNodeStatement> ScopedStatements { get; set; } = [];
}