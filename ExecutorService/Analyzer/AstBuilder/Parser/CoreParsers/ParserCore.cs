using ExecutorService.Analyzer._AnalyzerUtils;

namespace ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;

public class ParserCore(List<Token> tokens, FilePosition filePosition)
{
    protected Token ConsumeIfOfType(TokenType tokenType, string expectedTokenMsg)
    {
        var peekedToken = PeekToken();
        if (peekedToken != null && peekedToken.Type == tokenType)
        {
            return ConsumeToken();
        }
        throw new JavaSyntaxException($"Expected {expectedTokenMsg} declaration");
    }
    
    protected Token? PeekToken(int offset = 0)
    {
        var accessIndex = filePosition.GetFilePos() + offset;
        return accessIndex < tokens.Count ? tokens[accessIndex] : null;
    }
    
    protected Token ConsumeToken()
    {
        if (filePosition.GetFilePos() >= tokens.Count)
        {
            throw new InvalidOperationException("No more tokens");
        }

        var filePos = filePosition.GetFilePos();
        filePosition.IncrementFilePos();
        return tokens[filePos];
    }
    
    protected  bool CheckTokenType(TokenType tokenType, int offset = 0)
    {
        var peekedToken = PeekToken(offset);
        return peekedToken is not null && peekedToken.Type == tokenType;
    }
    protected Token TryConsume()
    {
        if (PeekToken() != null)
        {
            return ConsumeToken();
        }

        throw new JavaSyntaxException("token expected");
    }
    
    protected Token[] TryConsumeNTokens(int amount = 1)
    {
        var consumedTokens = new Token[amount];
        for (var i = 0; i < amount; i++)
        {
            consumedTokens[i] = TryConsume();
        }

        return consumedTokens;
    }
}