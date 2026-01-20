using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

// ReSharper disable once InconsistentNaming
public interface IInterfaceParser
{
    public AstNodeInterface ParseInterface(List<MemberModifier> legalModifiers);
    public AstNodeTypeScope<AstNodeInterface> ParseInterfaceScope(AstNodeInterface nodeInterface);
}