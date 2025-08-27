using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;

namespace ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

public interface IKeywordLexer
{
    public Token ConsumeKeyword(char triggerChar);

}