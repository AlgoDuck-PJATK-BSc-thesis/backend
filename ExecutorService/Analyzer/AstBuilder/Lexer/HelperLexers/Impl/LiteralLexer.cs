using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Lexer.CoreLexers;
using ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser;
using ExecutorService.Errors.Exceptions;

namespace ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers.Impl;

public class LiteralLexer(char[] fileChars, FilePosition filePosition, List<Token> tokens) : LexerCore(fileChars, filePosition, tokens), ILiteralLexer
{
    public Token ConsumeStringLit()
    {
        var stringLit = new StringBuilder();
        // check if this doesn't break if a file begins with '"' illegal statement so shouldn't pass either way but not because of a ArrayIndexOutOfBoundsException
        // which might get thrown. PeekChar should handle it but best to check
        //!CheckForChar('"') && !CheckForChar('\\', -1)
        while (!(CheckForChar('"') && !CheckForChar('\\', -1)))
        {
            stringLit.Append(ConsumeChar());
        }

        ConsumeChar(); // close string lit
        return CreateToken(TokenType.StringLit, stringLit.ToString());
    }

    public Token ConsumeCharLit()
    {
        ConsumeChar(); // consume opening ' 
        var charLit = new StringBuilder();

        // same case as in ConsumeStringLit()
        while (!(CheckForChar('\'') && !CheckForChar('\\', -1)))
        {
            charLit.Append(ConsumeChar());
        }

        ConsumeChar(); // consume closing '
        return CreateToken(TokenType.CharLit, charLit.ToString());
    }

    public Token ConsumeNumericLit(char consumed)
    {
        var numLit = new StringBuilder();
        char? workingChar = consumed;
        if (consumed == '-')
        {
            numLit.Append(consumed);
            workingChar = PeekChar();
        }

        if (workingChar == '0')
        {
            numLit.Append(ConsumeChar());
            workingChar = PeekChar();
            if (workingChar == null) throw new JavaSyntaxException("expected rest of expression");
            switch (workingChar.Value)
            {
                case 'b':
                case 'B':
                    return ConsumeBinLiteral(numLit);
                case 'x':
                case 'X':
                    return ConsumeHexLiteral(numLit);
                default:
                    return IsLegalChar(workingChar.Value, ILiteralLexer.OctalLiteralRange) ? ConsumeOctLiteral(numLit) : CreateToken(TokenType.IntLit, numLit.ToString());
            }

        }

        if (IsLegalChar(workingChar, ILiteralLexer.DecimalLiteralRange))
        {
            numLit.Append(ConsumeDec());
        }

        var delim = PeekChar();
        if (delim == '.')
        {
            numLit.Append(ConsumeChar());
            numLit.Append(ConsumeDec());
            delim = PeekChar();
        }

        if (delim == null) throw new JavaSyntaxException("rest od statement expected");
        
        switch (delim)
        {
            case 'f':
            case 'F':
                ConsumeChar();
                return CreateToken(TokenType.FloatLit, numLit.ToString());
            case 'e':
            case 'E':
                numLit.Append(ConsumeChar());
                if (CheckForChar('-'))
                {
                    numLit.Append(ConsumeChar());
                }
                numLit.Append(ConsumeDec());
                if (CheckForChar('f'))
                {
                    ConsumeChar();
                    return CreateToken(TokenType.FloatLit, numLit.ToString());
                }

                ConsumeChar();
                return CreateToken(TokenType.DoubleLit, numLit.ToString());
            case 'l':
            case 'L':
                ConsumeChar(); // I guess we could append this but from the perspective of ast building or generation we have all the necessary info in TokenType so it's enough to just consume
                return CreateToken(TokenType.LongLit, numLit.ToString());
            default: 
                return numLit.ToString().Contains('.') ? CreateToken(TokenType.DoubleLit, numLit.ToString()) : CreateToken(TokenType.IntLit, numLit.ToString());
        }
    }
    
    private Token ConsumeHexLiteral(StringBuilder numLit)
    {
        numLit.Append(ConsumeChar());
        numLit.Append(ConsumeHex());
        if (CheckForChar('.'))
        {
            numLit.Append(ConsumeChar());
            numLit.Append(ConsumeHex());
        }

        if (!CheckForChar('p')) return CreateToken(TokenType.IntLit, numLit.ToString());
        
        numLit.Append(ConsumeChar());
        if (CheckForChar('-'))
        {
            numLit.Append(ConsumeChar());
        }
        numLit.Append(ConsumeDec());
        if (CheckForChar('.'))
        {
            numLit.Append(ConsumeChar());
            numLit.Append(ConsumeDec());
        }

        if (!CheckForChar('f') && !CheckForChar('F')) return CreateToken(TokenType.DoubleLit, numLit.ToString());
        ConsumeChar();
        return CreateToken(TokenType.FloatLit, numLit.ToString());

    }

    private Token ConsumeOctLiteral(StringBuilder numLit)
    {
        numLit.Append(ConsumeOct());
        return CreateToken(TokenType.IntLit, numLit.ToString());
    }

    private Token ConsumeBinLiteral(StringBuilder numLit)
    {
        numLit.Append(ConsumeChar());
        numLit.Append(ConsumeBin());
        return CreateToken(TokenType.IntLit, numLit.ToString());
    }
    
    private string ConsumeBin() => ConsumeWhileLegalChar(ILiteralLexer.BinaryLiteralRange);
    private string ConsumeOct() => ConsumeWhileLegalChar(ILiteralLexer.OctalLiteralRange);
    private string ConsumeDec() => ConsumeWhileLegalChar(ILiteralLexer.DecimalLiteralRange);
    private string ConsumeHex() => ConsumeWhileLegalChar(ILiteralLexer.HexadecimalLiteralRange);
}