using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

namespace ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

public interface IGenericSettable
{
    public void SetGenericTypes(List<GenericTypeDeclaration> tokens);
}