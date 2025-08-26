namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

public class GenericTypeDeclaration
{
    public string GenericIdentifier { get; set; } = string.Empty;
    public List<Token> UpperBounds { get; set; } = [];
}