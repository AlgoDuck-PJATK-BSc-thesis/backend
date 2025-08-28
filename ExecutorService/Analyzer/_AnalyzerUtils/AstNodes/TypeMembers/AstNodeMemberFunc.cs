using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeMemberFunc<T> : IGenericSettable, ITypeMember<T> where T : IType<T>
{
    private T? Owner { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Default;
    public List<MemberModifier> Modifiers { get; set; } = [];
    public List<GenericTypeDeclaration> GenericTypes { get; set; } = [];
    public OneOf<MemberType,SpecialMemberType, ArrayType, ComplexTypeDeclaration>? FuncReturnType { get; set; } // same here
    public Token? Identifier { get; set; }
    public List<AstNodeScopeMemberVar> FuncArgs { get; set; } = [];
    public AstNodeStatementScope? FuncScope { get; set; }
    public bool IsConstructor { get; set; } = false;
    public List<Token> ThrownExceptions { get; set; } = [];

    public void SetGenericTypes(List<GenericTypeDeclaration> tokens)
    {
        GenericTypes = tokens;
    }

    public T? GetMemberType()
    {
        return Owner;
    }

    public void SetMemberType(T t)
    {
        Owner = t;
    }
}