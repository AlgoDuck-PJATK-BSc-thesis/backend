using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

public interface IType<T> where T: IType<T>
{
    Token? GetIdentifier();
    
    List<AstNodeTypeMember<T>> GetMembers();
    AstNodeTypeScope<T>? GetScope();
}