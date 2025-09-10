using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface IClassParser
{
    public AstNodeClass ParseClass(List<MemberModifier> legalModifiers);
    public AstNodeTypeScope<AstNodeClass> ParseClassScope(AstNodeClass clazz);
}