namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

public interface ITypeMember<T> where T: IType<T>
{
     T? GetMemberType();
     void SetMemberType(T t);

}

