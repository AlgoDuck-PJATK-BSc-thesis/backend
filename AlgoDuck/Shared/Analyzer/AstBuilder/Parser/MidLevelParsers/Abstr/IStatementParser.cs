using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;

public interface IStatementParser
{
    public AstNodeStatementScope ParseStatementScope();
    public AstNodeStatement? ParseStatement();
    public AstNodeStatement ParseDefaultStat();
    public AstNodeStatement ParseScopeWrapper();


}