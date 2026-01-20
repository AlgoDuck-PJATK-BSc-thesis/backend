using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface IClassParser
{
    public AstNodeClass ParseClass(List<MemberModifier> legalModifiers);
    public AstNodeTypeScope<AstNodeClass> ParseClassScope(AstNodeClass clazz);
}