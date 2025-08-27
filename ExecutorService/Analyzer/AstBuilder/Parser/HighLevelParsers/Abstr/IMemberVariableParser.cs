using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;

public interface IMemberVariableParser
{
    public AstNodeMemberVar<T> ParseMemberVariableDeclaration<T>(AstNodeTypeMember<T> typeMember) where T: IType<T>;

}