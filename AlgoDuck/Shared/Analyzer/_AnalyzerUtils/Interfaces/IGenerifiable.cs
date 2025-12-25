using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

public interface IGenerifiable
{
    public void SetGenericTypes(List<GenericTypeDeclaration> tokens);
}