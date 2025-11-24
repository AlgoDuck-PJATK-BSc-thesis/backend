using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeMemberVar<T> : ITypeMember<T> where T : IType<T>
{
    private T? Owner { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Default;
    public AstNodeScopeMemberVar ScopeMemberVar { get; set; } = new();
    public T? GetMemberType()
    {
        return Owner;
    }

    public void SetMemberType(T t)
    {
        Owner = t;
    }
}