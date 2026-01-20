using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class ClassParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    HighLevelParser(tokens, filePosition, symbolTableBuilder),
    IClassParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    public AstNodeClass
        ParseClass(
            List<MemberModifier> legalModifiers) // perhaps should not focus on grammatical correctness immediately but this is fairly low-hanging fruit
    {
        var nodeClass = new AstNodeClass();
        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier != null)
        {
            nodeClass.AccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        while (PeekToken()?.Type == TokenType.At)
        {
            nodeClass.AddAnnotation(ParseAnnotation());
        }

        nodeClass.ClassModifiers = ParseModifiers(legalModifiers);

        nodeClass.IsAbstract = nodeClass.ClassModifiers.Contains(MemberModifier.Abstract);

        ConsumeIfOfType("class", TokenType.Class);

        nodeClass.Name = ConsumeIfOfType("class name", TokenType.Ident);

        ParseGenericDeclaration(nodeClass);

        ParseExtendsKeyword(nodeClass);

        ParseImplementsKeyword(nodeClass);

        _symbolTableBuilder.DefineSymbol(new TypeSymbol<AstNodeClass>
        {
            AssociatedType = nodeClass,
            Name = nodeClass.Name.Value!,
        });

        nodeClass.TypeScope = ParseClassScope(nodeClass);
        return nodeClass;
    }

    public AstNodeTypeScope<AstNodeClass> ParseClassScope(AstNodeClass clazz)
    {
        _symbolTableBuilder.EnterScope();
        var classScope = new AstNodeTypeScope<AstNodeClass>
        {
            OwnScope = _symbolTableBuilder.CurrentScope,
            OwnerMember = clazz,
            ScopeBeginOffset = ConsumeIfOfType("'{'", TokenType.OpenCurly).FilePos
        };


        while (!CheckTokenType(TokenType.CloseCurly) && PeekToken() != null)
        {
            var prevToken =  PeekToken();
            try
            {
                var typeMemberParser = new TypeMemberParser(_tokens, _filePosition, _symbolTableBuilder);
                classScope.TypeMembers.Add(typeMemberParser.ParseTypeMember(clazz));
            }
            catch (JavaSyntaxException)
            {
                if (prevToken != null && PeekToken() == prevToken)
                {
                    ConsumeToken();
                }
            }
        }

        classScope.ScopeEndOffset = ConsumeIfOfType("'}'", TokenType.CloseCurly).FilePos;
        _symbolTableBuilder.ExitScope();
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