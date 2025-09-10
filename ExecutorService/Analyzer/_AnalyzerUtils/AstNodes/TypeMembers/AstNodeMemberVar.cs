using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

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