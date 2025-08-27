using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

// ReSharper disable once InconsistentNaming
public interface IInterfaceParser
{
    public AstNodeInterface ParseInterface(List<MemberModifier> legalModifiers);
    public AstNodeTypeScope<AstNodeInterface> ParseInterfaceScope(AstNodeInterface nodeInterface);
}