using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

/*
 *  TODO I recognize the similarities between this and ClassParser.
 *  I do however worry that if I try to extract some shared parts it might become abstracted to the point of obscuring functionality
 */
public class InterfaceParser(List<Token> tokens, FilePosition filePosition) :
    HighLevelParser(tokens, filePosition),
    IInterfaceParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
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

        ConsumeIfOfType(TokenType.Interface, "interface");
        
        nodeInterface.Identifier = ConsumeIfOfType(TokenType.Ident, "interface name");
        
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
            ScopeBeginOffset = ConsumeIfOfType(TokenType.OpenCurly, "'{'").FilePos,
        };
        while (!CheckTokenType(TokenType.CloseCurly))
        {
            interfaceScope.TypeMembers.Add(new TypeMemberParser(_tokens, _filePosition).ParseTypeMember(astNodeInterface));
        }
        interfaceScope.ScopeEndOffset = ConsumeIfOfType(TokenType.CloseCurly, "'}'").FilePos;
        return interfaceScope;
    }
    
    private void ParseExtendsKeyword(AstNodeInterface interfase)
    {
        if (!CheckTokenType(TokenType.Extends)) return;
        ConsumeToken(); // consume "extends"
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.OpenCurly)
        {
            interfase.Extends.Add(ParseComplexTypDeclaration());
            ConsumeIfOfType(TokenType.Comma, ",");
        }
        interfase.Extends.Add(ParseComplexTypDeclaration());
    }
}