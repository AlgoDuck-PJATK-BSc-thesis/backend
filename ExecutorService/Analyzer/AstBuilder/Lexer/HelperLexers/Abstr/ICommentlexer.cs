using ExecutorService.Analyzer.AstBuilder.Lexer.CoreLexers;

namespace ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

public interface ICommentlexer
{
    public void ConsumeMultiLineComment();
    public void ConsumeComment();

}