namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

public class GenericTypeDeclaration
{
    public string GenericIdentifier { get; set; } = string.Empty;
    public List<ComplexTypeDeclaration> UpperBounds { get; set; } = [];

    public override string ToString()
    {
        return UpperBounds.Count == 0 ? GenericIdentifier : $"{GenericIdentifier} extends {string.Join(" & ", UpperBounds)}";
    }
}
