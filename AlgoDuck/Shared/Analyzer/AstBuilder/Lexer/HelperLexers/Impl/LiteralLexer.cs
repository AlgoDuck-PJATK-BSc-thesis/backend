using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.CoreLexers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Impl;

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

        ConsumeChar(); 
        return CreateToken(TokenType.StringLit, stringLit.ToString());
    }

    public Token ConsumeCharLit()
    {
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
        
        char? workingChar = consumed;
    
        if (workingChar == '0')
        {
            workingChar = PeekChar();
            if (workingChar == null) throw new JavaSyntaxException("Expected token");
        
            switch (workingChar.Value)
            {
                case 'b':
                case 'B':
                    return ConsumeBinLiteral();
                case 'x':
                case 'X':
                    return ConsumeHexLiteral();
                default:
                    return IsLegalChar(workingChar.Value, ILiteralLexer.OctalLiteralRange) 
                        ? ConsumeOctLiteral() 
                        : CreateToken(TokenType.IntLit, "0");
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
    
        if (delim == null) throw new JavaSyntaxException("expected token");
        
        switch (delim)
        {
            case 'f':
            case 'F':
                ConsumeChar();
                return CreateToken(TokenType.FloatLit, NormalizeFloat(numLit.ToString()));
                
            case 'e':
            case 'E':
                ConsumeChar(); // consume 'e' or 'E'
                return ConsumeScientificNotation(numLit.ToString());
                
            case 'l':
            case 'L':
                ConsumeChar();
                return CreateToken(TokenType.LongLit, NormalizeLong(numLit.ToString()));
                
            default: 
                return numLit.ToString().Contains('.') 
                    ? CreateToken(TokenType.DoubleLit, NormalizeDouble(numLit.ToString())) 
                    : CreateToken(TokenType.IntLit, NormalizeInt(numLit.ToString()));
        }
    }

    private Token ConsumeScientificNotation(string baseValue)
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
            return CreateToken(TokenType.FloatLit, result.ToString("R"));
        }
        
        var baseDouble = double.Parse(baseValue);
        var resultDouble = baseDouble * Math.Pow(10, exponent);
        return CreateToken(TokenType.DoubleLit, resultDouble.ToString("R"));
    }

    private Token ConsumeHexLiteral()
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
                return CreateToken(TokenType.LongLit, NormalizeLong(integerPart, 16));
            }
            return CreateToken(TokenType.IntLit, NormalizeInt(integerPart, 16));
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
            return CreateToken(TokenType.FloatLit, ((float)result).ToString("R"));
        }
        
        return CreateToken(TokenType.DoubleLit, result.ToString("R"));
    }

    private Token ConsumeOctLiteral()
    {
        var octalDigits = ConsumeOct();
        if (string.IsNullOrEmpty(octalDigits))
        {
            return CreateToken(TokenType.IntLit, "0");
        }
        
        if (CheckForChar('l') || CheckForChar('L'))
        {
            ConsumeChar();
            return CreateToken(TokenType.LongLit, NormalizeLong("0" + octalDigits, 8));
        }
        
        return CreateToken(TokenType.IntLit, NormalizeInt("0" + octalDigits, 8));
    }
    
    private Token ConsumeBinLiteral()
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
            return CreateToken(TokenType.LongLit, NormalizeLong(binaryDigits, 2));
        }
        
        return CreateToken(TokenType.IntLit, NormalizeInt(binaryDigits, 2));
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