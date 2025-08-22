using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;

public class MemberVariableParser(List<Token> tokens, FilePosition filePosition) : 
    MidLevelParser(tokens, filePosition),
    IMemberVariableParser
{

    public AstNodeClassMemberVar ParseMemberVariableDeclaration(AstNodeClassMember classMember)
    {
        AstNodeClassMemberVar memberVar = new()
        {
            ClassMember  = classMember
        };
        var accessModifier = TokenIsAccessModifier(PeekToken());
        memberVar.AccessModifier = accessModifier ?? AccessModifier.Public;
        if (accessModifier is not null)
        {
            ConsumeToken();
        }
        memberVar.ScopeMemberVar = ParseScopeMemberVariableDeclaration([MemberModifier.Final, MemberModifier.Static]);
        return memberVar;
    }
}