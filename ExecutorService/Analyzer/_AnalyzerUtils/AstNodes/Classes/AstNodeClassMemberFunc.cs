using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClassMemberFunc
{
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public List<MemberModifier> Modifiers { get; set; } = new();
    public List<Token> GenericTypes { get; set; } = new(); // TODO Idk if tokens here are super optimal, probably should wrap them in some customType node
    public OneOf<MemberType,SpecialMemberType, ArrayType, Token>? FuncReturnType { get; set; } // same here
    public Token? Identifier { get; set; }
    public List<AstNodeScopeMemberVar> FuncArgs { get; set; } = new();
    public AstNodeStatementScope? FuncScope { get; set; }
}