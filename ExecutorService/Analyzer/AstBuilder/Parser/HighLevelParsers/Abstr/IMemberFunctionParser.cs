using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;

public interface IMemberFunctionParser
{
    public AstNodeMemberFunc<T> ParseMemberFunctionDeclaration<T>(AstNodeTypeMember<T> typeMember) where T: IType<T>;
    public void ParseMemberFuncReturnType<T>(AstNodeMemberFunc<T> memberFunc) where T: IType<T>;
    public void ParseMemberFunctionArguments<T>(AstNodeMemberFunc<T> memberFunc) where T: IType<T>;



}