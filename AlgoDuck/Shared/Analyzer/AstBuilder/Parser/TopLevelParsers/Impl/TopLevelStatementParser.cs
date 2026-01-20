using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class TopLevelStatementParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    HighLevelParser(tokens, filePosition, symbolTableBuilder),
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

        var classParser = new ClassParser(_tokens, _filePosition, symbolTableBuilder);
        var interfaceParser = new InterfaceParser(_tokens, _filePosition, symbolTableBuilder);
        return PeekToken(lookahead)!.Type switch
        {
            TokenType.Class => classParser.ParseClass([MemberModifier.Final, MemberModifier.Static, MemberModifier.Abstract]),
            TokenType.Interface => interfaceParser.ParseInterface([MemberModifier.Abstract, MemberModifier.Strictfp]),
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