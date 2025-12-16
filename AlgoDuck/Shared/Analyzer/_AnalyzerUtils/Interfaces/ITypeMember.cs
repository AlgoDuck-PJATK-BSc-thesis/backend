using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

public interface ITypeMember<T> where T:  BaseType<T>
{
     T? GetMemberType();
     void SetMemberType(T t);

}

