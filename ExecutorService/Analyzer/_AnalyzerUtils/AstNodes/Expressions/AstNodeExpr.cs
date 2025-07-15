using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Expressions;

public class AstNodeExpr
{
    public OneOf<AstNodeBinExpr, AstNodeUnaryExpr, AstNodeExprIdent>? Variant { get; set; }
}