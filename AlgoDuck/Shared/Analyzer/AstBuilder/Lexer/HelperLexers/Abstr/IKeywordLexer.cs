using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

public interface IKeywordLexer
{
    public Token ConsumeKeyword(char triggerChar);

}