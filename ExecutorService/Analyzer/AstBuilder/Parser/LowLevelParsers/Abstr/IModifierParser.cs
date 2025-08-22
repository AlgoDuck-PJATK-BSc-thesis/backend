using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface IModifierParser
{
    public AccessModifier? TokenIsAccessModifier(Token? token);
    public bool TokenIsModifier(Token token);
    public List<MemberModifier> ParseModifiers(List<MemberModifier> legalModifiers);



}