using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer;


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
        var lexer = new LexerSimple(fileContentsSanitized.ToCharArray(), FilePosition.GetFilePosition(out _), []);
        return lexer.Tokenize();
    }
    
    private List<Token> Tokenize()
    {
        while (PeekChar() != null)
        {
            var consumedChar = ConsumeChar();
            switch (consumedChar)
            {
                case '/':
                    HandleForwardSlash();
                    break;
                case '{':
                    _tokens.Add(CreateToken(TokenType.OpenCurly, _filePosition.GetFilePos() - 1));
                    break;
                case '}':
                    _tokens.Add(CreateToken(TokenType.CloseCurly, _filePosition.GetFilePos() - 1));
                    break;
                case '[':
                    _tokens.Add(CreateToken(TokenType.OpenBrace, _filePosition.GetFilePos() - 1));
                    break;
                case ']':
                    _tokens.Add(CreateToken(TokenType.CloseBrace, _filePosition.GetFilePos() - 1));
                    break;
                case '(':
                    _tokens.Add(CreateToken(TokenType.OpenParen, _filePosition.GetFilePos() - 1));
                    break;
                case ')':
                    _tokens.Add(CreateToken(TokenType.CloseParen, _filePosition.GetFilePos() - 1));
                    break;
                case '=':
                    HandleEqual();
                    break;
                case ';':
                    _tokens.Add(CreateToken(TokenType.Semi, _filePosition.GetFilePos() - 1));
                    break;
                case ':':
                    HandleColon();
                    break;
                case '.':
                    _tokens.Add(CreateToken(TokenType.Dot, _filePosition.GetFilePos() - 1));
                    break;
                case ',':
                    _tokens.Add(CreateToken(TokenType.Comma, _filePosition.GetFilePos() - 1));
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
                    HandleMul();
                    break;
                case '%':
                    HandleMod();
                    break;
                case '<':
                    HandleOpenChevron();
                    break;
                case '>':
                    HandleCloseChevron();                    
                    break;
                case '?':
                    _tokens.Add(CreateToken(TokenType.Wildcard, _filePosition.GetFilePos() - 1));
                    break;
                case '&':
                    HandleAnd();
                    break;
                case '|':
                    HandleOr();
                    break;
                case '^':
                    HandleXor();
                    break;
                case '!':
                    HandleNegation();
                    break;
                case '~':
                    _tokens.Add(CreateToken(TokenType.Tilde, _filePosition.GetFilePos() - 1));
                    break;
                case '@':
                    _tokens.Add(CreateToken(TokenType.At, _filePosition.GetFilePos() - 1));
                    break;
                default:
                    HandleDefaultCase(consumedChar);
                    break;
            }
        }
        
        return _tokens;
    }

    private void HandleColon()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar(':'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.DoubleColon, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Colon, startPos));
        }
    }

    private void HandleNegation()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Neq, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Negation, startPos));
        }
    }
    
    private void HandleOr()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.BitOrAssign, startPos));
        }
        else if (CheckForChar('|'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.LogOr, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.BitOr, startPos));
        }
    }

    private void HandleAnd()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.BitAndAssign, startPos));
        }
        else if (CheckForChar('&'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.LogAnd, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.BitAnd, startPos));
        }
    }


    private void HandleXor()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.BitXorAssign, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.BitXor, startPos));
        }
    }
    
    private void HandleMul()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.MulAssign, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Mul, startPos));
        }
    }


    private void HandleMod()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.ModAssign, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Mod, startPos));
        }
    }
    
    private void HandleOpenChevron()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Le, startPos));
        }
        else if (CheckForChar('<'))
        {
            ConsumeChar();
            if (CheckForChar('='))
            {
                ConsumeChar();
                _tokens.Add(CreateToken(TokenType.LBitShiftAssign, startPos));
            }
            else
            {
                _tokens.Add(CreateToken(TokenType.LBitShift, startPos));
            }
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.OpenChevron, startPos));
        }
    }
    
    private void HandleCloseChevron()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Ge, startPos));
        }
        else if (CheckForChar('>'))
        {
            ConsumeChar();
            if (CheckForChar('='))
            {
                ConsumeChar();
                _tokens.Add(CreateToken(TokenType.RBitShiftAssign, startPos));
            }
            else if (CheckForChar('>'))
            {   
                ConsumeChar();
                if (CheckForChar('='))
                {
                    ConsumeChar();
                    _tokens.Add(CreateToken(TokenType.UrBitShiftAssign, startPos));
                }
                else
                {
                    _tokens.Add(CreateToken(TokenType.UrBitShift, startPos));
                }
            }
            else
            {
                _tokens.Add(CreateToken(TokenType.RBitShift, startPos));
            }
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.CloseChevron, startPos));
        }    
    }
    
    private void HandleForwardSlash()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        if (CheckForChar('/'))
        {
            ConsumeComment();
        }
        else if (CheckForChar('*'))
        {
            ConsumeMultiLineComment();
        }
        else if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.DivAssign, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Div, startPos));
        }
    }

    private void HandleEqual()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Eq, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Assign, startPos));
        }
    }

    private void HandlePlus()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('+'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Increment, startPos));
        }
        else if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.PlusAssign, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Plus, startPos));
        }  
    }
    
    private void HandleMinus()
    {
        var startPos = _filePosition.GetFilePos() - 1;
        
        if (CheckForChar('-'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Decrement, startPos));
        }
        else if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.MinusAssign, startPos));
        }
        else if (CheckForChar('>'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Arrow, startPos));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Minus, startPos));
        } 
    }

    private void HandleDefaultCase(char consumedChar)
    {
        if (char.IsNumber(consumedChar))
        {
            _tokens.Add(ConsumeNumericLit(consumedChar));
        }
        else if (char.IsLetter(consumedChar) || consumedChar == '_')
        {
            _tokens.Add(ConsumeKeyword(consumedChar));
        }
        else if (char.IsWhiteSpace(consumedChar))
        {
            // just skip
        }
    }
}