namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

public class ComplexTypeDeclaration
{
    public string Identifier { get; set; } = string.Empty;
    public List<GenericInitialization>? GenericInitializations { get; set; }
}

public class GenericInitialization
{
    public ComplexTypeDeclaration? Identifier { get; set; }
    public bool IsWildCard { get; set; } = false;
    public ComplexTypeDeclaration? SupersType { get; set; }
    public List<ComplexTypeDeclaration>? ExtendsTypes { get; set; }
}