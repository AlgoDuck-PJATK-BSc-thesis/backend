using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;

public interface IMemberFunctionParser
{
    public AstNodeMemberFunc<T> ParseMemberFunctionDeclaration<T>(AstNodeTypeMember<T> typeMember) where T: IType<T>;
    public void ParseMemberFuncReturnType<T>(AstNodeMemberFunc<T> memberFunc) where T: IType<T>;
    public void ParseMemberFunctionArguments<T>(AstNodeMemberFunc<T> memberFunc) where T: IType<T>;



}