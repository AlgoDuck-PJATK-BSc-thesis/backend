using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;

public class ParserCore(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder)
{
    protected Token ConsumeIfOfType(string expectedTokenMsg, params TokenType[] tokenType)
    {
        var peekedToken = PeekToken();
        if (peekedToken == null) throw new JavaSyntaxException(expectedTokenMsg);
        
        if (tokenType.Any(type => peekedToken.Type == type))
        {
            return ConsumeToken();
        }
        
        throw new JavaSyntaxException(expectedTokenMsg);
    }

    protected bool SkipIfOfType(TokenType expectedTokenType)
    {
        return TryConsumeIfOfType(expectedTokenType, out var _);
    }

    protected bool TryConsumeIfOfType(TokenType expectedTokenType, out Token? token)
    {
        token = null;
        if (!CheckTokenType(expectedTokenType)) return false;
        token = ConsumeToken();
        return true;
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