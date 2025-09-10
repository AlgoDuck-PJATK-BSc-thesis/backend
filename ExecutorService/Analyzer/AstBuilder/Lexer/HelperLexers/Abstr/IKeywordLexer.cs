using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;

namespace ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

public interface IKeywordLexer
{
    public Token ConsumeKeyword(char triggerChar);

}