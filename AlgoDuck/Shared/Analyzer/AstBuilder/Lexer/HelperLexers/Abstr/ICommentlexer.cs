namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

public interface ICommentlexer
{
    public void ConsumeMultiLineComment();
    public void ConsumeComment();

}