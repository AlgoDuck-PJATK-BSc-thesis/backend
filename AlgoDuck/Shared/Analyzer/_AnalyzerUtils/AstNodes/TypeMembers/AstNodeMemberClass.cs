using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeMemberClass<T> : ITypeMember<T> where T: IType<T>
{
    private T? OwnerMember { get; set; }
    public AstNodeClass? Class { get; set; }

    public T? GetMemberType()
    {
        return OwnerMember;
    }

    public void SetMemberType(T t)
    {
        OwnerMember = t;
    }
}