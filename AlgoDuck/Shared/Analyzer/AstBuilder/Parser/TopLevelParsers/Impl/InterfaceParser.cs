using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

/*
 *  TODO I recognize the similarities between this and ClassParser.
 *  I do however worry that if I try to extract some shared parts it might become abstracted to the point of obscuring functionality
 */
public class InterfaceParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    HighLevelParser(tokens, filePosition, symbolTableBuilder),
    IInterfaceParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;
    
    public AstNodeInterface ParseInterface(List<MemberModifier> legalModifiers)
    {
        var nodeInterface = new AstNodeInterface();

        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier != null)
        {
            nodeInterface.InterfaceAccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        nodeInterface.Modifiers = ParseModifiers(legalModifiers);

        ConsumeIfOfType("interface", TokenType.Interface);
        
        nodeInterface.Identifier = ConsumeIfOfType("interface name", TokenType.Ident);
        
        ParseGenericDeclaration(nodeInterface);
        
        ParseExtendsKeyword(nodeInterface);

        nodeInterface.InterfaceScope = ParseInterfaceScope(nodeInterface);
        return nodeInterface;
    }

    public AstNodeTypeScope<AstNodeInterface> ParseInterfaceScope(AstNodeInterface astNodeInterface)
    {
        var interfaceScope = new AstNodeTypeScope<AstNodeInterface>()
        {
            OwnerMember = astNodeInterface,
            ScopeBeginOffset = ConsumeIfOfType("'{'", TokenType.OpenCurly).FilePos,
        };
        while (!CheckTokenType(TokenType.CloseCurly))
        {
            interfaceScope.TypeMembers.Add(new TypeMemberParser(_tokens, _filePosition, _symbolTableBuilder).ParseTypeMember(astNodeInterface));
        }
        interfaceScope.ScopeEndOffset = ConsumeIfOfType("'}'", TokenType.CloseCurly).FilePos;
        return interfaceScope;
    }
    
    private void ParseExtendsKeyword(AstNodeInterface interfase)
    {
        if (!CheckTokenType(TokenType.Extends)) return;
        ConsumeToken(); // consume "extends"
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.OpenCurly)
        {
            interfase.Extends.Add(ParseComplexTypDeclaration());
            ConsumeIfOfType(",", TokenType.Comma);
        }
        interfase.Extends.Add(ParseComplexTypDeclaration());
    }
}