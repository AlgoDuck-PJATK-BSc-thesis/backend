namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodePackage : IHasUriSetter
{
    public string Uri { get; set; } = string.Empty;
    
    
    public void SetUri(string uri)
    {
        Uri = uri;
    }
}