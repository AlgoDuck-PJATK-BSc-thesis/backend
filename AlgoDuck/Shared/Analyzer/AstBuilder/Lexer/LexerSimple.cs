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
                case ':':
                    HandleColon();
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
                    _tokens.Add(CreateToken(TokenType.Wildcard));
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
                    _tokens.Add(CreateToken(TokenType.Tilde));
                    break;
                case '@':
                    _tokens.Add(CreateToken(TokenType.At));
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
        if (CheckForChar(':'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.DoubleColon));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Colon));
        }
    }

    private void HandleNegation()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Neq));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Negation));
        }
    }
    
    private void HandleOr()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.BitOrAssign));
        }
        else if (CheckForChar('|'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.LogOr));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.BitOr));
        }
    }

    private void HandleAnd()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.BitAndAssign));
        }
        else if (CheckForChar('&'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.LogAnd));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.BitAnd));
        }
    }


    private void HandleXor()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.BitXorAssign));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.BitXor));
        }
    }
    
    private void HandleMul()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.MulAssign));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Mul));
        }
    }


    private void HandleMod()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.ModAssign));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Mod));
        }
    }
    
    private void HandleOpenChevron()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Le));
        }
        else if (CheckForChar('<'))
        {
            ConsumeChar();
            if (CheckForChar('='))
            {
                ConsumeChar();
                _tokens.Add(CreateToken(TokenType.LBitShiftAssign));
            }
            else
            {
                _tokens.Add(CreateToken(TokenType.LBitShift));
            }
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.OpenChevron));
        }
    }
    
    private void HandleCloseChevron()
    {
        if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Ge));
        }
        else if (CheckForChar('>'))
        {
            ConsumeChar();
            if (CheckForChar('='))
            {
                ConsumeChar();
                _tokens.Add(CreateToken(TokenType.RBitShiftAssign));
            }
            else if (CheckForChar('>'))
            {   
                ConsumeChar();
                if (CheckForChar('='))
                {
                    ConsumeChar();
                    _tokens.Add(CreateToken(TokenType.UrBitShiftAssign));
                }
                else
                {
                    _tokens.Add(CreateToken(TokenType.UrBitShift));
                }
            }
            else
            {
                _tokens.Add(CreateToken(TokenType.RBitShift));
            }
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.CloseChevron));
        }    
    }
    
    private void HandleForwardSlash()
    {
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
            _tokens.Add(CreateToken(TokenType.DivAssign));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Div));
        }
    }

    private void HandleEqual()
    {
        if (CheckForChar('='))
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
        if (CheckForChar('+'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Increment));
        }
        else if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.PlusAssign));
        }
        else
        {
            _tokens.Add(CreateToken(TokenType.Plus));
        }  
    }
    
    private void HandleMinus()
    {
        if (CheckForChar('-'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Decrement));
        }
        else if (CheckForChar('='))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.MinusAssign));
        }
        else if (CheckForChar('>'))
        {
            ConsumeChar();
            _tokens.Add(CreateToken(TokenType.Arrow));
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