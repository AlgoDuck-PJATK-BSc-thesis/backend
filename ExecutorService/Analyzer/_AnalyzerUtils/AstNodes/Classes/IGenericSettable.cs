using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public interface IGenericSettable
{
    public void SetGenericTypes(List<GenericTypeDeclaration> tokens);
}