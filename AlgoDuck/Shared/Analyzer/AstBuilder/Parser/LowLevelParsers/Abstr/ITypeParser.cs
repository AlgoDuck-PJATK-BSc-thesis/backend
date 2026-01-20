using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface ITypeParser
{
    public OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration> ParseType();
    public OneOf<MemberType, ArrayType, ComplexTypeDeclaration> ParseStandardType();
    
    public bool TokenIsSimpleType(Token? token);
    public MemberType ParseSimpleType(Token token);
    public ComplexTypeDeclaration ParseComplexTypDeclaration();


}