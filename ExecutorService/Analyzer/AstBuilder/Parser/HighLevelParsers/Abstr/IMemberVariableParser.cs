using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;

public interface IMemberVariableParser
{
    public AstNodeClassMemberVar ParseMemberVariableDeclaration(AstNodeClassMember classMember);

}