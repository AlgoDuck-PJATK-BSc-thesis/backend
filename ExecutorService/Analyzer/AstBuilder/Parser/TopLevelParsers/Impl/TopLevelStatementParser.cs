using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class TopLevelStatementParser(List<Token> tokens, FilePosition filePosition) :
    HighLevelParser(tokens, filePosition),
    ITopLevelStatementParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;

    public OneOf<AstNodeClass> ParseTopLevelStatement()
    {
        var lookahead = 0;
        while (!(CheckTokenType(TokenType.Class, lookahead) /*|| CheckTokenType(TokenType.Import, lookahead)*/)) 
        {
            lookahead++;
        }

        return PeekToken(lookahead)!.Type switch
        {
            TokenType.Class => new ClassParser(_tokens, _filePosition).ParseClass([MemberModifier.Final, MemberModifier.Static]),
            _ => throw new JavaSyntaxException($"Unexpected token: {PeekToken(lookahead)!.Type}")
        };
    }
    
    public void ParseImportsAndPackages(IHasUriSetter statement)
    {
        var uri = new StringBuilder();
        
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.Semi)
        {
            var uriComponent = ConsumeIfOfType(TokenType.Ident, "uri component").Value!;
            uri.Append($"{uriComponent}.");
            ConsumeToken(); // consume delim
        }

        var lastUriComponentToken = ConsumeIfOfType(TokenType.Ident, "identifier");
        ConsumeIfOfType(TokenType.Semi, "semi colon");
        uri.Append(lastUriComponentToken.Value!);

        statement.SetUri(uri.ToString());
    }
}