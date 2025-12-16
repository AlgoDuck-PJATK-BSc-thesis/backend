using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;

public interface IMemberFunctionParser
{
    public AstNodeMemberFunc<T> ParseMemberFunctionDeclaration<T>(AstNodeTypeMember<T> typeMember) where T:  BaseType<T>;
    public void ParseMemberFuncReturnType<T>(AstNodeMemberFunc<T> memberFunc) where T:  BaseType<T>;
    public void ParseMemberFunctionArguments<T>(AstNodeMemberFunc<T> memberFunc) where T:  BaseType<T>;



}