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
        const int maxLookahead = 10000;

        while (PeekToken(lookahead) != null &&
               !(CheckTokenType(TokenType.Class, lookahead) ||
                 CheckTokenType(TokenType.Interface, lookahead) || 
                 CheckTokenType(TokenType.Enum, lookahead) || 
                 CheckTokenType(TokenType.Record, lookahead) ||
                 (CheckTokenType(TokenType.At, lookahead) && CheckTokenType(TokenType.Interface, lookahead + 1))))
        {
            lookahead++;
            if (lookahead >= maxLookahead)
            {
                throw new JavaParseComplexityExceededException("Member declaration too complex");
            }
        }

        if (PeekToken(lookahead) == null)
        {
            throw new JavaSyntaxException("Unexpected end of input while looking for type definition");
        }

        var classParser = new ClassParser(_tokens, _filePosition, symbolTableBuilder);
        var interfaceParser = new InterfaceParser(_tokens, _filePosition, symbolTableBuilder);
        
        return PeekToken(lookahead)!.Type switch
        {
            TokenType.Class => classParser.ParseClass([MemberModifier.Final, MemberModifier.Static, MemberModifier.Abstract]),
            TokenType.Interface => interfaceParser.ParseInterface([MemberModifier.Abstract, MemberModifier.Strictfp]),
            TokenType.Enum => SkipUnsupportedTypeDefinition(lookahead, "Enums"),
            TokenType.Record => SkipUnsupportedTypeDefinition(lookahead, "Records"),
            TokenType.At => SkipUnsupportedTypeDefinition(lookahead, "Annotation types"), // @interface
            _ => throw new JavaSyntaxException($"Unexpected token: {PeekToken(lookahead)!.Type}")
        };
    }

    private OneOf<AstNodeClass, AstNodeInterface> SkipUnsupportedTypeDefinition(int lookahead, string typeName)
    {
        for (var i = 0; i < lookahead; i++)
        {
            ConsumeToken();
        }
        
        while (!CheckTokenType(TokenType.OpenBrace) && PeekToken() != null)
        {
            ConsumeToken();
        }
        
        SkipBraceBlock();
        throw new JavaSyntaxException($"{typeName} are not supported");
    }
    
    private void SkipBraceBlock()
    {
        if (!CheckTokenType(TokenType.OpenBrace))
        {
            return;
        }
        
        ConsumeToken(); // consume '{'
        
        var braceDepth = 1;
        while (braceDepth > 0 && PeekToken() != null)
        {
            if (CheckTokenType(TokenType.OpenBrace))
            {
                braceDepth++;
            }
            else if (CheckTokenType(TokenType.CloseBrace))
            {
                braceDepth--;
            }
            ConsumeToken();
        }
    }
    
    public void ParseImportsAndPackages(IHasUriSetter statement)
    {
        var uri = new StringBuilder();
        const int maxUriComponents = 100; 
        var componentCount = 0;
        
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.Semi)
        {
            if (++componentCount > maxUriComponents)
            {
                throw new JavaSyntaxException("Package/import path too deep");
            }
            
            if (CheckTokenType(TokenType.Ident) || CheckTokenType(TokenType.Mul))
            {
                uri.Append($"{ConsumeToken().Value}.");
            }
            else
            {
                throw new JavaSyntaxException("Illegal uri component");
            }
            
            if (!CheckTokenType(TokenType.Dot))
            {
                throw new JavaSyntaxException("Expected '.' in package/import path");
            }
            ConsumeToken(); // consume dot
        }

        if (!CheckTokenType(TokenType.Ident))
        {
            throw new JavaSyntaxException("Expected identifier at end of package/import");
        }
        
        var lastUriComponentToken = ConsumeIfOfType("identifier", TokenType.Ident);
        ConsumeIfOfType("semi colon", TokenType.Semi);
        uri.Append(lastUriComponentToken.Value!);

        statement.SetUri(uri.ToString());
    }
}