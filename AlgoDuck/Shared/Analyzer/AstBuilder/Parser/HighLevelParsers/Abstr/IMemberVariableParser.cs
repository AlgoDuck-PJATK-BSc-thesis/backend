using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;

public interface IMemberVariableParser
{
    public AstNodeMemberVar<T> ParseMemberVariableDeclaration<T>(AstNodeTypeMember<T> typeMember) where T:  BaseType<T>;

}