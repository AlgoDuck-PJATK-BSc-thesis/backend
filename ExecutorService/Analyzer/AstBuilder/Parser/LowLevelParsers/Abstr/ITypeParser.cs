using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface ITypeParser
{
    public OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration> ParseType();
    public bool TokenIsSimpleType(Token? token);
    public MemberType ParseSimpleType(Token token);
    public ComplexTypeDeclaration ParseComplexTypDeclaration();


}