using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;

public interface IScopeVariableParser
{
    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers);

}