using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;

public class AstNodeStatementUnknown
{
    public Token Ident { get; }

    public AstNodeStatementUnknown(Token ident)
    {
        Ident = ident;
    }
}