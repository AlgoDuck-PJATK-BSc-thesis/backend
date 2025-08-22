using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;

namespace ExecutorService.Analyzer.AstBuilder.Lexer;

public interface ILexer
{
    public List<Token> Tokenize(string fileContents);
}

public class LexerSimple : ILexer
{
    private char[] _fileChars;
    private int _currPos;
    private StringBuilder _buf;
    private List<Token> _tokens;
    
    public List<Token> Tokenize(string fileContents)
    {
        fileContents = fileContents.ReplaceLineEndings();
         _tokens = new List<Token>();
        _fileChars = fileContents.ToCharArray();
        _buf = new StringBuilder();
        _currPos = 0;

        while (PeekChar() != null)
        {
            char consumedChar = ConsumeChar();
            switch (consumedChar)
            {
                case '/':
                    if (CheckForChar('/', 1))
                    {
                        ConsumeComment();
                    }else if (CheckForChar('*', 1))
                    {
                        ConsumeMultiLineComment();
                    }
                    break;
                case '{':
                    _tokens.Add(CreateToken(TokenType.OpenCurly));
                    break;
                case '}':
                    _tokens.Add(CreateToken(TokenType.CloseCurly));
                    break;
                case '[':
                    _tokens.Add(CreateToken(TokenType.OpenBrace));
                    break;
                case ']':
                    _tokens.Add(CreateToken(TokenType.CloseBrace));
                    break;
                case '(':
                    _tokens.Add(CreateToken(TokenType.OpenParen));
                    break;
                case ')':
                    _tokens.Add(CreateToken(TokenType.CloseParen));
                    break;
                case '=':
                    _tokens.Add(CreateToken(TokenType.Assign));
                    break;
                case ';':
                    _tokens.Add(CreateToken(TokenType.Semi));
                    break;
                case '.':
                    _tokens.Add(CreateToken(TokenType.Dot));
                    break;
                case ',':
                    _tokens.Add(CreateToken(TokenType.Comma));
                    break;
                case '"':
                    _tokens.Add(ConsumeStringLit());
                    break;
                case '\'':
                    _tokens.Add(ConsumeCharLit());
                    break;
                case '-':
                    _tokens.Add(CreateToken(TokenType.Minus));
                    break;
                case '<':
                    _tokens.Add(CreateToken(TokenType.OpenChevron));
                    break;
                case '>':
                    _tokens.Add(CreateToken(TokenType.CloseChevron));
                    break;
                default:
                    if (Char.IsNumber(consumedChar))
                    {
                        _tokens.Add(ConsumeNumericLit(consumedChar));
                    }
                    else if (Char.IsLetter(consumedChar))
                    {
                        _buf.Append(consumedChar);
                        _tokens.Add(ConsumeKeyword(_buf));
                    }else if (Char.IsWhiteSpace(consumedChar))
                    {
                        
                    }
                    break;
            }
        }
        return _tokens;
    }
    
    private Token ConsumeKeyword(StringBuilder buf)
    {
        while (PeekChar() != null && Char.IsLetterOrDigit(PeekChar()!.Value))
        {
            buf.Append(ConsumeChar());
        }

        var result = buf.ToString();
        var token = result switch
        {
            "private" => CreateToken(TokenType.Private),
            "public" => CreateToken(TokenType.Public),
            "protected" => CreateToken(TokenType.Protected),
            "void" => CreateToken(TokenType.Void),
            "byte" => CreateToken(TokenType.Byte),
            "short" => CreateToken(TokenType.Short),
            "int" => CreateToken(TokenType.Int),
            "long" => CreateToken(TokenType.Long),
            "float" => CreateToken(TokenType.Float),
            "double" => CreateToken(TokenType.Double),
            "char" => CreateToken(TokenType.Char),
            "boolean" => CreateToken(TokenType.Boolean),
            "static" => CreateToken(TokenType.Static),
            "final" => CreateToken(TokenType.Final),
            "class" => CreateToken(TokenType.Class),
            "String" => CreateToken(TokenType.String),
            "import" => CreateToken(TokenType.Import),
            _ => CreateToken(TokenType.Ident, result),
        };
        buf.Clear();

        return token;
    }

    private void ConsumeComment()
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

    private void ConsumeMultiLineComment()
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
    
private Token ConsumeNumericLit(char prevChar) // TODO needs cleaning up, bit of a monstrosity atm
    {
        StringBuilder numLit = new StringBuilder();
        if (CheckForChar('-'))
        {
            numLit.Append(ConsumeChar());
        }

        char peekedLitTypeChar = prevChar; // if 0 then either octal, bin or hex decimal
        char? peekedLitValStartChar = PeekChar(); // first numeric value of literal
        if (peekedLitTypeChar is '0' && peekedLitValStartChar.HasValue)
        {

            numLit.Append(prevChar);
            switch (peekedLitValStartChar.Value)
            {
                case 'b':
                case 'B':
                    numLit.Append(ConsumeChar());
                    numLit.Append(ConsumeBin());
                    break;
                case 'x':
                case 'X':
                    numLit.Append(ConsumeChar());
                    numLit.Append(ConsumeHex());
                    if (CheckForChar('.'))
                    {
                        numLit.Append(ConsumeChar());
                        numLit.Append(ConsumeHex());
                    }
                    
                    if (CheckForChar('p'))
                    {
                        numLit.Append(ConsumeChar());
                        if (CheckForChar('-')) // todo make consumeIfOfType()
                        {
                            numLit.Append(ConsumeChar());
                        }
                        numLit.Append(ConsumeDec());
                        if (CheckForChar('.'))
                        {
                            numLit.Append(ConsumeChar());
                            numLit.Append(ConsumeDec());
                        }
                        if (CheckForChar('f') || CheckForChar('F')) // todo use toLowerCase() here, just don't know in what context as CheckForCharProbably shouldn't auto lowercase, perhaps separate method? 
                        {
                            ConsumeChar();
                            return CreateToken(TokenType.FloatLit, numLit.ToString());
                        }
                        return CreateToken(TokenType.DoubleLit, numLit.ToString());
                    }
                    break;
                default:
                    numLit.Append(ConsumeOct());
                    break;
            }

            return CreateToken(TokenType.IntLit, numLit.ToString());
        }

        if (char.IsNumber(prevChar))
        {
            numLit.Append(prevChar);
        }
        
        numLit.Append(ConsumeDec());
        var delim = PeekChar();
        if (delim is not null && delim.Value == '.')
        {
            numLit.Append(ConsumeChar());
            numLit.Append(ConsumeDec());
            delim = PeekChar();
        }
        
        if (delim is not null)
        {
            numLit.Append(ConsumeDec());
            delim = PeekChar();
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
        throw new JavaSyntaxException("idk man");
    }

    private string ConsumeBin() => ConsumeWhileLegalChar(new int[][] { [48, 49] });
    private string ConsumeOct() => ConsumeWhileLegalChar(new int[][] { [48, 55] });
    private string ConsumeDec() => ConsumeWhileLegalChar(new int[][] { [48, 57] });
    private string ConsumeHex() => ConsumeWhileLegalChar(new int[][] { [48, 57], [65, 70], [97, 102] });
    
    
    private String ConsumeWhileLegalChar(int[][] legalCharRanges)
    {
        char? peekedChar = PeekChar();
        StringBuilder consumed = new StringBuilder();
        while (peekedChar is not null)
        {
            int peekedCharNum = (int)peekedChar;
            foreach (var legalRange in legalCharRanges)
            {
                if (peekedCharNum >= legalRange[0] && peekedCharNum <= legalRange[1])
                {
                    consumed.Append(ConsumeChar());
                    peekedChar = PeekChar();
                    goto validCharacter;
                }
            }

            break;
            validCharacter: ;
        }

        return consumed.ToString();
    }

    private Token ConsumeStringLit()
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

    private Token ConsumeCharLit()
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

    private bool CheckForChar(char checkedChar, int offset = 0)
    {
        return PeekChar(offset) == checkedChar;
    }
    private char? PeekChar(int offset = 0)
    {
        var accessIndex = offset + _currPos;
        if (accessIndex < _fileChars.Length && accessIndex >= 0)
        {
            return _fileChars[accessIndex];
        }

        return null;
    }
    
    private Token CreateToken(TokenType type, string? value = null)
    {
        return new Token(type, _currPos - 1, value);
    }
    private char ConsumeChar()
    {
        return _fileChars[_currPos++];
    }
}