using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;
using OneOf;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeTypeMember<T> : ITypeMember<T> where T : BaseType<T>
{
    private T? Owner { get; set; }

    public OneOf<AstNodeMemberFunc<T>, AstNodeMemberVar<T>, AstNodeMemberClass<T>> ClassMember { get; set; }
    public T? GetMemberType()
    {
        return Owner;
    }

    public void SetMemberType(T t)
    {
        Owner = t;
    }
}