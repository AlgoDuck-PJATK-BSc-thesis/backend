using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface IExpressionParser
{
    public NodeExpr ParseExpr(int minPrecedence = 1);
    // public NodeTerm? ParseTerm();

}