using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;

public class AstNodeStatement
{
    public OneOf<AstNodeStatementScope, AstNodeStatementUnknown> Variant { get; set; }
}