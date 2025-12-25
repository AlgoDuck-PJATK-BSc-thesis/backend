
namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

public class Token(TokenType type, int filePos , string? value = null)
{

    public Token(TokenType type, string? value = null) : this(type, int.MaxValue, value)
    {
        
    }
    public TokenType Type => type;

    public string? Value => value;

    public int FilePos => filePos;
}