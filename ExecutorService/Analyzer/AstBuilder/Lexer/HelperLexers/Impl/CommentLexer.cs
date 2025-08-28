using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Lexer.CoreLexers;
using ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser;

namespace ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Impl;

public class CommentLexer(char[] fileChars, FilePosition filePosition, List<Token> tokens) : LexerCore(fileChars, filePosition, tokens), ICommentlexer
{
    public void ConsumeMultiLineComment()
    {
        ConsumeChar(); // consume '/'
        ConsumeChar(); // consume '/*'
        
        while (PeekChar() != null && !(CheckForChar('*') && CheckForChar('/', 1)))
        {
            ConsumeChar();
        }

        if (PeekChar() is not null)
        {
            ConsumeChar(); // consume '*'
        }
        if (PeekChar() is not null)
        {
            ConsumeChar(); // consume '/'
        }
    }

    public void ConsumeComment()
    {
        ConsumeChar(); // consume '/'
        ConsumeChar(); // consume '/'
        while (PeekChar() != null && !(CheckForChar('\n') || CheckForChar('\r')))
        {
            ConsumeChar();
        }

        ConsumeChar(); // consume '\n' or '\r'
        if (CheckForChar('\n')) // for windows
        {
            ConsumeChar(); 
        }
    }
}