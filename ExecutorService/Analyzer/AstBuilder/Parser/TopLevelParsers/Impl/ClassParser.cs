using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class ClassParser(List<Token> tokens, FilePosition filePosition) :
    HighLevelParser(tokens, filePosition),
    IClassParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;

    public AstNodeClass ParseClass(List<MemberModifier> legalModifiers) // perhaps should not focus on grammatical correctness immediately but this is fairly low-hanging fruit
    {
        var nodeClass = new AstNodeClass();
        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier != null)
        {
            nodeClass.ClassAccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        nodeClass.ClassModifiers = ParseModifiers(legalModifiers);

        nodeClass.IsAbstract = nodeClass.ClassModifiers.Contains(MemberModifier.Abstract);
        
        ConsumeIfOfType("class", TokenType.Class);

        nodeClass.Identifier = ConsumeIfOfType("class name", TokenType.Ident);

        ParseGenericDeclaration(nodeClass);

        ParseExtendsKeyword(nodeClass);
        
        ParseImplementsKeyword(nodeClass);
        
        nodeClass.ClassScope = ParseClassScope(nodeClass);
        return nodeClass;
    }
    
    public AstNodeTypeScope<AstNodeClass> ParseClassScope(AstNodeClass clazz)
    {

        var classScope = new AstNodeTypeScope<AstNodeClass>
        {
            OwnerMember = clazz,
            ScopeBeginOffset = ConsumeIfOfType("'{'", TokenType.OpenCurly).FilePos
        };
        
        while (!CheckTokenType(TokenType.CloseCurly))
        {
            classScope.TypeMembers.Add(new TypeMemberParser(_tokens, _filePosition).ParseTypeMember(clazz));
        }
        
        classScope.ScopeEndOffset = ConsumeIfOfType("'}'", TokenType.CloseCurly).FilePos;
        return classScope;
    }

    private void ParseExtendsKeyword(AstNodeClass clazz)
    {
        if (!CheckTokenType(TokenType.Extends)) return;
        ConsumeToken(); // consume "extends"
        clazz.Extends = ParseComplexTypDeclaration();
    }

    private void ParseImplementsKeyword(AstNodeClass clazz)
    {
        if (!CheckTokenType(TokenType.Implements)) return;
        ConsumeToken(); // consume "implements"
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.OpenCurly)
        {
            clazz.Implements.Add(ParseComplexTypDeclaration());
            ConsumeIfOfType(",", TokenType.Comma);
        }
        clazz.Implements.Add(ParseComplexTypDeclaration());
    }
}