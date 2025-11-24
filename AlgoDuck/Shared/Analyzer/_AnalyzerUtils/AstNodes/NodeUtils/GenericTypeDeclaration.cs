namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

public class GenericTypeDeclaration
{
    public string GenericIdentifier { get; set; } = string.Empty;
    public List<ComplexTypeDeclaration> UpperBounds { get; set; } = [];
}