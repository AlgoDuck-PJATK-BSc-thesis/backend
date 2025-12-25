using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;

public class MemberVariableParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : 
    MidLevelParser(tokens, filePosition, symbolTableBuilder),
    IMemberVariableParser
{
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    public AstNodeMemberVar<T> ParseMemberVariableDeclaration<T>(AstNodeTypeMember<T> typeMember) where T:  BaseType<T>
    {
        var memberVar = new AstNodeMemberVar<T>()
        {
            DeclaredScope = _symbolTableBuilder.CurrentScope
            
        };
        memberVar.SetMemberType(typeMember.GetMemberType()!);

        while (PeekToken()?.Type == TokenType.At)
        {
            memberVar.AddAnnotation(ParseAnnotation());
        }
        
        var accessModifier = TokenIsAccessModifier(PeekToken());
        memberVar.AccessModifier = TokenIsAccessModifier(PeekToken()) ?? AccessModifier.Default;
        
        if (accessModifier != null)
        {
            ConsumeToken();
        }
        
        memberVar.ScopeMemberVar = ParseScopeMemberVariableDeclaration([MemberModifier.Final, MemberModifier.Static]);
        
        return memberVar;
    }
}