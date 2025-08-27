using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

namespace ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

public interface IType<T> where T: IType<T>
{
    Token? GetIdentifier();
    
    List<AstNodeTypeMember<T>> GetMembers();
    AstNodeTypeScope<T>? GetScope();
}