namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodePackage : IUriSetter
{
    public string Uri { get; set; } = string.Empty;
    
    
    public void SetUri(string uri)
    {
        Uri = uri;
    }
}