using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;

namespace ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;

public interface IScopeVariableParser
{
    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers);

}