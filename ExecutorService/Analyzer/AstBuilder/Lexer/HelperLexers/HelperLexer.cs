using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Lexer.CoreLexers;
using ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Impl;
using ExecutorService.Analyzer.AstBuilder.Parser;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;

namespace ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers;

public class HelperLexer(char[] fileChars, FilePosition filePosition, List<Token> tokens) : 
    LexerCore(fileChars, filePosition, tokens), 
    IKeywordLexer, ILiteralLexer, ICommentlexer
{
    private readonly CommentLexer _commentLexer = new(fileChars, filePosition, tokens);
    private readonly LiteralLexer _literalLexer = new(fileChars, filePosition, tokens);
    private readonly KeywordLexer _keywordLexer = new(fileChars, filePosition, tokens);
    
    public Token ConsumeKeyword(char triggerChar)
    {
        return _keywordLexer.ConsumeKeyword(triggerChar);
    }

    public Token ConsumeStringLit()
    {
        return _literalLexer.ConsumeStringLit();
    }

    public Token ConsumeCharLit()
    {
        return _literalLexer.ConsumeCharLit();
    }

    public Token ConsumeNumericLit(char consumed)
    {
        return _literalLexer.ConsumeNumericLit(consumed);
    }

    public void ConsumeMultiLineComment()
    {
        _commentLexer.ConsumeMultiLineComment();
    }

    public void ConsumeComment()
    {
        _commentLexer.ConsumeComment();
    }
}