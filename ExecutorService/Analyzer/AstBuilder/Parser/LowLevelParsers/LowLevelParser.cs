using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers;

public class LowLevelParser(List<Token> tokens, FilePosition filePosition) :
    ParserCore(tokens, filePosition),
    IGenericParser, IModifierParser, ITypeParser
{
    private GenericParser _genericParser = new(tokens, filePosition);
    private ModifierParser _modifierParser = new(tokens, filePosition);
    private TypeParser _typeParser = new(tokens, filePosition);

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
}