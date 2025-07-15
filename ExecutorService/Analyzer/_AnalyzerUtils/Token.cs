
namespace ExecutorService.Analyzer._AnalyzerUtils;

public class Token(TokenType type, int filePos , string? value = null)
{
    public TokenType Type => type;

    public string? Value => value;

    public int FilePos => filePos;
}