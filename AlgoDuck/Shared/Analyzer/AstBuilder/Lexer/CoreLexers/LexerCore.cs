using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.CoreLexers;

public abstract class LexerCore(char[] fileChars, FilePosition filePosition, List<Token> tokens)
{
    private readonly char[] _fileChars = fileChars;
    private readonly FilePosition _filePosition = filePosition;
    private List<Token> _tokens = tokens;

    protected string ConsumeWhileLegalChar(int[][] legalCharRanges)
    {
        var consumed = new StringBuilder();
        while (PeekChar() != null && IsLegalChar(PeekChar(), legalCharRanges))
        {
            consumed.Append(ConsumeChar());
        }

        return consumed.ToString();
    }
    
    protected bool IsLegalChar(char? peekedChar, int[][] legalCharRanges)
    {
        if (peekedChar == null) return false;
        var peekedCharNum = (int) peekedChar;
        return legalCharRanges.Any(legalRange => peekedCharNum >= legalRange[0] && peekedCharNum <= legalRange[1]);
    }
    
    protected bool CheckForChar(char checkedChar, int offset = 0)
    {
        return PeekChar(offset) == checkedChar;
    }
    protected char? PeekChar(int offset = 0)
    {
        var accessIndex = offset + _filePosition.GetFilePos();
        if (accessIndex < _fileChars.Length && accessIndex >= 0)
        {
            return _fileChars[accessIndex];
        }

        return null;
    }
    
    protected Token CreateToken(TokenType type, string? value = null)
    {
        return new Token(type, _filePosition.GetFilePos() - 1, value);
    }
    protected char ConsumeChar()
    {
        var currPos = _filePosition.GetFilePos();
        _filePosition.IncrementFilePos();
        return _fileChars[currPos];
    }
}