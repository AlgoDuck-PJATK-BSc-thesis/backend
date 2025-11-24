using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers;

public class LowLevelParser(List<Token> tokens, FilePosition filePosition) :
    ParserCore(tokens, filePosition),
    IGenericParser, IModifierParser, ITypeParser, IExpressionParser
{
    private GenericParser _genericParser = new(tokens, filePosition);
    private ModifierParser _modifierParser = new(tokens, filePosition);
    private TypeParser _typeParser = new(tokens, filePosition);
    private ExpressionParser _expressionParser = new(tokens, filePosition);
    
    public void ParseGenericDeclaration(IGenericSettable funcOrClass)
    {
        _genericParser.ParseGenericDeclaration(funcOrClass);
    }

    public AccessModifier? TokenIsAccessModifier(Token? token)
    {
        return _modifierParser.TokenIsAccessModifier(token);
    }

    public bool TokenIsModifier(Token token)
    {
        return _modifierParser.TokenIsModifier(token);
    }

    public List<MemberModifier> ParseModifiers(List<MemberModifier> legalModifiers)
    {
        return _modifierParser.ParseModifiers(legalModifiers);
    }

    public OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration> ParseType()
    {
        return _typeParser.ParseType();
    }

    public bool TokenIsSimpleType(Token? token)
    {
        return _typeParser.TokenIsSimpleType(token);
    }

    public MemberType ParseSimpleType(Token token)
    {
        return _typeParser.ParseSimpleType(token);
    }

    public ComplexTypeDeclaration ParseComplexTypDeclaration()
    {
        return _typeParser.ParseComplexTypDeclaration();
    }

    public NodeExpr ParseExpr(int minPrecedence = 1)
    {
        return _expressionParser.ParseExpr(minPrecedence);
    }

    // public NodeTerm? ParseTerm()
    // {
    //     return _expressionParser.ParseTerm();
    // }
}