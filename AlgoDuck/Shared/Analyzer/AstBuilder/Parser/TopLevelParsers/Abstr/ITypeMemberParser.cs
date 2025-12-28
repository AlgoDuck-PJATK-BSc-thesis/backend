using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface ITypeMemberParser
{
    public AstNodeTypeMember<T> ParseTypeMember<T>(T member) where T : BaseType<T>;

}