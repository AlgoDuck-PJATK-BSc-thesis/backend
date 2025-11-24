using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;

public class MemberVariableParser(List<Token> tokens, FilePosition filePosition) : 
    MidLevelParser(tokens, filePosition),
    IMemberVariableParser
{

    public AstNodeMemberVar<T> ParseMemberVariableDeclaration<T>(AstNodeTypeMember<T> typeMember) where T: IType<T>
    {
        var memberVar = new AstNodeMemberVar<T>();
        memberVar.SetMemberType(typeMember.GetMemberType()!);
        var accessModifier = TokenIsAccessModifier(PeekToken());
        memberVar.AccessModifier = accessModifier ?? AccessModifier.Default;
        if (accessModifier is not null)
        {
            ConsumeToken();
        }
        memberVar.ScopeMemberVar = ParseScopeMemberVariableDeclaration([MemberModifier.Final, MemberModifier.Static]);
        return memberVar;
    }
}