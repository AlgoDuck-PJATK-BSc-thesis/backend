using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface IClassParser
{
    public AstNodeClass ParseClass(List<MemberModifier> legalModifiers);
    public AstNodeCLassScope ParseClassScope(AstNodeClass clazz);
}