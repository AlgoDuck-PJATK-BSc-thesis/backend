using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using ExecutorService.Errors.Exceptions;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class TopLevelStatementParser(List<Token> tokens, FilePosition filePosition) :
    HighLevelParser(tokens, filePosition),
    ITopLevelStatementParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;

    public OneOf<AstNodeClass, AstNodeInterface> ParseTypeDefinition()
    {
        var lookahead = 0;
        while (!(CheckTokenType(TokenType.Class, lookahead) || CheckTokenType(TokenType.Interface, lookahead))) 
        {
            lookahead++;
        }

        return PeekToken(lookahead)!.Type switch
        {
            TokenType.Class => new ClassParser(_tokens, _filePosition).ParseClass([MemberModifier.Final, MemberModifier.Static, MemberModifier.Abstract]),
            TokenType.Interface => new InterfaceParser(_tokens, _filePosition).ParseInterface([MemberModifier.Abstract, MemberModifier.Strictfp]),
            _ => throw new JavaSyntaxException($"Unexpected token: {PeekToken(lookahead)!.Type}")
        };
    }
    
    public void ParseImportsAndPackages(IHasUriSetter statement)
    {
        var uri = new StringBuilder();
        
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.Semi)
        {
            if (CheckTokenType(TokenType.Ident) || CheckTokenType(TokenType.Mul))
            {
                uri.Append($"{ConsumeToken().Value}.");
            }
            else
            {
                throw new JavaSyntaxException("Illegal uri component");
            }
            ConsumeToken(); // consume delim
        }

        var lastUriComponentToken = ConsumeIfOfType("identifier", TokenType.Ident);
        ConsumeIfOfType("semi colon", TokenType.Semi);
        uri.Append(lastUriComponentToken.Value!);

        statement.SetUri(uri.ToString());
    }
}