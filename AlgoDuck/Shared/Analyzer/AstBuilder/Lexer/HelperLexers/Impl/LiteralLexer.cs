using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.CoreLexers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Impl;

public class LiteralLexer(char[] fileChars, FilePosition filePosition, List<Token> tokens) : LexerCore(fileChars, filePosition, tokens), ILiteralLexer
{
    private readonly FilePosition _filePosition = filePosition;
    public Token ConsumeStringLit()
    {
        var start = _filePosition.GetFilePos() - 1;
        var stringLit = new StringBuilder();
        // check if this doesn't break if a file begins with '"' illegal statement so shouldn't pass either way but not because of a ArrayIndexOutOfBoundsException
        // which might get thrown. PeekChar should handle it but best to check
        //!CheckForChar('"') && !CheckForChar('\\', -1)
        while (!(CheckForChar('"') && !CheckForChar('\\', -1)))
        {
            stringLit.Append(ConsumeChar());
        }

        ConsumeChar(); 
        return CreateToken(TokenType.StringLit, start, stringLit.ToString());
    }

    public Token ConsumeCharLit()
    {
        var start = _filePosition.GetFilePos() - 1;
        var charLit = new StringBuilder();

        // same case as in ConsumeStringLit()
        while (!(CheckForChar('\'') && !CheckForChar('\\', -1)))
        {
            charLit.Append(ConsumeChar());
        }

        ConsumeChar(); // consume closing '
        return CreateToken(TokenType.CharLit, start, charLit.ToString());
    }

    public Token ConsumeNumericLit(char consumed)
    {
        var start = _filePosition.GetFilePos() - 1;
        char? workingChar = consumed;
    
        if (workingChar == '0')
        {
            workingChar = PeekChar();
            if (workingChar == null) throw new JavaSyntaxException("Expected token");
        
            switch (workingChar.Value)
            {
                case 'b':
                case 'B':
                    return ConsumeBinLiteral(start);
                case 'x':
                case 'X':
                    return ConsumeHexLiteral(start);
                default:
                    return IsLegalChar(workingChar.Value, ILiteralLexer.OctalLiteralRange) 
                        ? ConsumeOctLiteral(start) 
                        : CreateToken(TokenType.IntLit, start, "0");
            }
        }
    
        var numLit = new StringBuilder(consumed.ToString());
        
        if (IsLegalChar(workingChar, ILiteralLexer.DecimalLiteralRange))
        {
            numLit.Append(ConsumeDec());
        }
    
        var delim = PeekChar();
        if (delim == '.')
        {
            numLit.Append(ConsumeChar()); // consume '.'
            if (PeekChar() != null && IsLegalChar(PeekChar(), ILiteralLexer.DecimalLiteralRange))
            {
                numLit.Append(ConsumeDec());
            }
            delim = PeekChar();
        }
    
        switch (delim)
        {
            case 'f':
            case 'F':
                ConsumeChar();
                return CreateToken(TokenType.FloatLit, start, NormalizeFloat(numLit.ToString()));
                
            case 'e':
            case 'E':
                ConsumeChar(); // consume 'e' or 'E'
                return ConsumeScientificNotation(numLit.ToString(), start);
                
            case 'l':
            case 'L':
                ConsumeChar();
                return CreateToken(TokenType.LongLit, start, NormalizeLong(numLit.ToString()));
                
            default: 
                return numLit.ToString().Contains('.') 
                    ? CreateToken(TokenType.DoubleLit, start, NormalizeDouble(numLit.ToString())) 
                    : CreateToken(TokenType.IntLit, start, NormalizeInt(numLit.ToString()));
        }
    }

    private Token ConsumeScientificNotation(string baseValue, int start)
    {
        
        var exponentBuilder = new StringBuilder();
        if (CheckForChar('-'))
        {
            exponentBuilder.Append(ConsumeChar());
        }
        
        if (PeekChar() == null || !IsLegalChar(PeekChar(), ILiteralLexer.DecimalLiteralRange))
        {
            throw new JavaSyntaxException("Expected exponent after 'e'");
        }
        
        exponentBuilder.Append(ConsumeDec());
        var exponent = int.Parse(exponentBuilder.ToString());
        
        if (CheckForChar('f') || CheckForChar('F'))
        {
            ConsumeChar();
            var baseFloat = float.Parse(baseValue);
            var result = (float)(baseFloat * Math.Pow(10, exponent));
            return CreateToken(TokenType.FloatLit, start, result.ToString("R"));
        }
        
        var baseDouble = double.Parse(baseValue);
        var resultDouble = baseDouble * Math.Pow(10, exponent);
        return CreateToken(TokenType.DoubleLit, start, resultDouble.ToString("R"));
    }

    private Token ConsumeHexLiteral(int start)
    {
        ConsumeChar(); // consume 'x' or 'X'
    
        var integerPart = ConsumeHex();
        if (string.IsNullOrEmpty(integerPart))
        {
            throw new JavaSyntaxException("Expected hex digits after 0x");
        }
        
        if (!CheckForChar('.'))
        {
            if (CheckForChar('l') || CheckForChar('L'))
            {
                ConsumeChar();
                return CreateToken(TokenType.LongLit, start, NormalizeLong(integerPart, 16));
            }
            return CreateToken(TokenType.IntLit, start, NormalizeInt(integerPart, 16));
        }
        
        ConsumeChar(); // consume '.'
        var fractionalPart = ConsumeHex();
        
        if (!CheckForChar('p') && !CheckForChar('P'))
        {
            throw new JavaSyntaxException("p exponent expected");
        }
        
        ConsumeChar(); // consume 'p' or 'P'
        
        var exponentBuilder = new StringBuilder();
        if (CheckForChar('-'))
        {
            exponentBuilder.Append(ConsumeChar());
        }
        
        if (PeekChar() == null || !IsLegalChar(PeekChar(), ILiteralLexer.DecimalLiteralRange))
        {
            throw new JavaSyntaxException("Expected exponent after 'p'");
        }
        
        exponentBuilder.Append(ConsumeDec());
        var exponent = int.Parse(exponentBuilder.ToString());
        
        var integerValue = string.IsNullOrEmpty(integerPart) ? 0 : Convert.ToInt64(integerPart, 16);
        var mantissa = 0.0;
        
        if (!string.IsNullOrEmpty(fractionalPart))
        {
            var fracInt = Convert.ToInt64(fractionalPart, 16);
            mantissa = fracInt / Math.Pow(16, fractionalPart.Length);
        }
        
        var baseValue = integerValue + mantissa;
        var result = baseValue * Math.Pow(2, exponent);
        
        if (CheckForChar('f') || CheckForChar('F'))
        {
            ConsumeChar();
            return CreateToken(TokenType.FloatLit, start, ((float)result).ToString("R"));
        }
        
        return CreateToken(TokenType.DoubleLit, start, result.ToString("R"));
    }

    private Token ConsumeOctLiteral(int start)
    {
        var octalDigits = ConsumeOct();
        if (string.IsNullOrEmpty(octalDigits))
        {
            return CreateToken(TokenType.IntLit, start, "0");
        }
        
        if (CheckForChar('l') || CheckForChar('L'))
        {
            ConsumeChar();
            return CreateToken(TokenType.LongLit, start, NormalizeLong("0" + octalDigits, 8));
        }
        
        return CreateToken(TokenType.IntLit, start, NormalizeInt("0" + octalDigits, 8));
    }
    
    private Token ConsumeBinLiteral(int start)
    {
        ConsumeChar(); // consume 'b' or 'B'
        var binaryDigits = ConsumeBin();
        
        if (string.IsNullOrEmpty(binaryDigits))
        {
            throw new JavaSyntaxException("Expected binary digits after 0b");
        }
        
        if (CheckForChar('l') || CheckForChar('L'))
        {
            ConsumeChar();
            return CreateToken(TokenType.LongLit, start, NormalizeLong(binaryDigits, 2));
        }
        
        return CreateToken(TokenType.IntLit, start, NormalizeInt(binaryDigits, 2));
    }
    
    private string NormalizeDouble(string value)
    {
        try
        {
            return double.Parse(value).ToString("R");
        }
        catch (FormatException)
        {
            throw new JavaSyntaxException($"Invalid double literal: {value}");
        }
    }
    
    private string NormalizeInt(string value, int baseValue = 10)
    {
        try
        {
            var parsed = Convert.ToInt32(value, baseValue);
            return parsed.ToString();
        }
        catch (FormatException)
        {
            throw new JavaSyntaxException($"Invalid integer literal: {value}");
        }
    }
    
    private string NormalizeLong(string value, int baseValue = 10)
    {
        try
        {
            var parsed = Convert.ToInt64(value, baseValue);
            return parsed.ToString();
        }
        catch (FormatException)
        {
            throw new JavaSyntaxException($"Invalid long literal: {value}");
        }
    }
    
    private string NormalizeFloat(string value)
    {
        try
        {
            return float.Parse(value).ToString("R");
        }
        catch (FormatException)
        {
            throw new JavaSyntaxException($"Invalid float literal: {value}");
        }
    }
    
    private string ConsumeBin() => ConsumeWhileLegalChar(ILiteralLexer.BinaryLiteralRange);
    private string ConsumeOct() => ConsumeWhileLegalChar(ILiteralLexer.OctalLiteralRange);
    private string ConsumeDec() => ConsumeWhileLegalChar(ILiteralLexer.DecimalLiteralRange);
    private string ConsumeHex() => ConsumeWhileLegalChar(ILiteralLexer.HexadecimalLiteralRange);
}