using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface IClassMemberParser
{
    public AstNodeClassMember ParseClassMember(AstNodeCLassScope classScope);
}