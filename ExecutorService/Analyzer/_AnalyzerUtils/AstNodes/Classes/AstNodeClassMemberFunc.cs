using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClassMemberFunc : IGenericSettable
{
    public AstNodeClassMember? OwnerClassMember { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public List<MemberModifier> Modifiers { get; set; } = [];
    public List<Token> GenericTypes { get; set; } = []; // TODO Idk if tokens here are super optimal, probably should wrap them in some customType node
    public OneOf<MemberType,SpecialMemberType, ArrayType, Token>? FuncReturnType { get; set; } // same here
    public Token? Identifier { get; set; }
    public List<AstNodeScopeMemberVar> FuncArgs { get; set; } = [];
    public AstNodeStatementScope? FuncScope { get; set; }
    public bool IsConstructor { get; set; } = false;

    public void SetGenericTypes(List<Token> tokens)
    {
        GenericTypes = tokens;
    }
}