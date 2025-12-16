using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

public interface IType<T> where T: BaseType<T>
{
    public Token? GetIdentifier();
    public List<AstNodeTypeMember<T>> GetMembers();
    public AstNodeTypeScope<T>? GetScope();
}