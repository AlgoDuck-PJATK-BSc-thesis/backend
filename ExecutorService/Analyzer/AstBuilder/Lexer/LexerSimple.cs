using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Lexer.HelperLexers;
using ExecutorService.Analyzer.AstBuilder.Parser;

namespace ExecutorService.Analyzer.AstBuilder.Lexer;


public class LexerSimple : 
    HelperLexer
{
    private char[] _fileChars;
    private FilePosition _filePosition;
    private readonly List<Token> _tokens;

    private LexerSimple(char[] fileChars, FilePosition filePosition, List<Token> tokens) : base(fileChars, filePosition, tokens)
    {
        _fileChars = fileChars;
        _filePosition = filePosition;
        _tokens = tokens;
    }

    public static List<Token> Tokenize(string fileContents)
    {
        var fileContentsSanitized = fileContents.ReplaceLineEndings();
        var lexer = new LexerSimple(fileContentsSanitized.ToCharArray(), new FilePosition(), []);
        return lexer.Tokenize();
    }
    
    private List<Token> Tokenize()
    {
        while (PeekChar() != null)
        {
            char consumedChar = ConsumeChar();
            switch (consumedChar)
            {
                case '/':
                    HandleForwardSlash();
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
                    HandleEqual();
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
                    HandleMinus();
                    break;
                case '+':
                    HandlePlus();
                    break;
                case '*':
                    _tokens.Add(CreateToken(TokenType.Mul));
                    break;
                case '%':
                    _tokens.Add(CreateToken(TokenType.Mod));
                    break;
                case '<':
                    HandleOpenChevron();
                    break;
                case '>':
                    HandleCloseChevron();                    
                    break;
                case '?':
                    _tokens.Add(CreateToken(TokenType.Wildcard));
                    break;
                case '&':
                    _tokens.Add(CreateToken(TokenType.And));
                    break;
                case '|':
                    _tokens.Add(CreateToken(TokenType.Or));
                    break;
                case '^':
                    _tokens.Add(CreateToken(TokenType.Xor));
                    break;
                default:
                    HandleDefaultCase(consumedChar);
                    break;
            }
        }
        
        return _tokens;
    }

    private void HandleOpenChevron()
    {
        if (CheckForChar('=', 1))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Le));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.OpenChevron));
        }
    }
    
    private void HandleCloseChevron()
    {
        if (CheckForChar('=', 1))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Ge));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.CloseChevron));
        }    
    }
    
    private void HandleForwardSlash()
    {
        if (CheckForChar('/', 1))
        {
            ConsumeComment();
        }else if (CheckForChar('*', 1))
        {
            ConsumeMultiLineComment();
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Div));
        }
    }

    private void HandleEqual()
    {
        if (CheckForChar('=', 1))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Eq));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Assign));
        }
    }

    private void HandlePlus()
    {
        if (CheckForChar('+', 1))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Increment));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Plus));
        }  
    }
    
    private void HandleMinus()
    {
        if (CheckForChar('-', 1))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Decrement));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Minus));
        } 
    }

    private void HandleDefaultCase(char consumedChar)
    {
        if (char.IsNumber(consumedChar))
        {
            _tokens.Add(ConsumeNumericLit(consumedChar));
        }
        else if (char.IsLetter(consumedChar))
        {
            _tokens.Add(ConsumeKeyword(consumedChar));
        }else if (char.IsWhiteSpace(consumedChar))
        {
            // just skip
        }
    }
}