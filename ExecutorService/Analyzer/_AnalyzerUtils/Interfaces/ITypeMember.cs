using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

namespace ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

public interface ITypeMember<T> where T: IType<T>
{
     T? GetMemberType();
     void SetMemberType(T t);

}

