using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
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
        AstNodeClass nodeClass = new();
        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier != null)
        {
            nodeClass.ClassAccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        nodeClass.ClassModifiers = ParseModifiers(legalModifiers);

        nodeClass.IsAbstract = nodeClass.ClassModifiers.Contains(MemberModifier.Abstract);
        
        ConsumeIfOfType(TokenType.Class, "class");

        nodeClass.Identifier = ConsumeIfOfType(TokenType.Ident, "class name");

        ParseGenericDeclaration(nodeClass);

        ParseExtendsKeyword(nodeClass);
        
        ParseImplementsKeyword(nodeClass);
        
        nodeClass.ClassScope = ParseClassScope(nodeClass);
        return nodeClass;
    }
    
    public AstNodeCLassScope ParseClassScope(AstNodeClass clazz)
    {

        AstNodeCLassScope classScope = new()
        {
            OwnerClass = clazz,
            ScopeBeginOffset = ConsumeIfOfType(TokenType.OpenCurly, "'{'").FilePos
        };
        
        while (!CheckTokenType(TokenType.CloseCurly))
        {
            classScope.ClassMembers.Add(new ClassMemberParser(_tokens, _filePosition).ParseClassMember(classScope));
        }
        
        classScope.ScopeEndOffset = ConsumeIfOfType(TokenType.CloseCurly, "'}'").FilePos;
        return classScope;
    }

    private void ParseExtendsKeyword(AstNodeClass clazz)
    {
        if (!CheckTokenType(TokenType.Extends)) return;
        ConsumeToken();
        clazz.Extends = ConsumeIfOfType(TokenType.Ident, "extended class");
    }

    private void ParseImplementsKeyword(AstNodeClass clazz)
    {
        if (!CheckTokenType(TokenType.Implements)) return;
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.OpenCurly)
        {
            clazz.Implements.Add(ConsumeIfOfType(TokenType.Ident, "implemented interface"));
            ConsumeIfOfType(TokenType.Comma, ",");
        }
        clazz.Implements.Add(ConsumeIfOfType(TokenType.Ident, "implemented interface"));
    }
}