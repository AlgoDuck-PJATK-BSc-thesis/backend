using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface IModifierParser
{
    public AccessModifier? TokenIsAccessModifier(Token? token);
    public bool TokenIsModifier(Token token);
    public List<MemberModifier> ParseModifiers(List<MemberModifier> legalModifiers);



}