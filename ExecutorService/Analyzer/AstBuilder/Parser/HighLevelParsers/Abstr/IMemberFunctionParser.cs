using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;

public interface IMemberFunctionParser
{
    public AstNodeClassMemberFunc ParseMemberFunctionDeclaration(AstNodeClassMember classMember);
    public void ParseMemberFuncReturnType(AstNodeClassMemberFunc memberFunc);
    public void ParseMemberFunctionArguments(AstNodeClassMemberFunc memberFunc);



}