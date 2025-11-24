using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface IExpressionParser
{
    public NodeExpr ParseExpr(int minPrecedence = 1);
    // public NodeTerm? ParseTerm();

}