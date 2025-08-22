using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClass : IGenericSettable
{
    public AccessModifier ClassAccessModifier { get; set; } = AccessModifier.Private;
    public List<MemberModifier> ClassModifiers { get; set; } = new();
    public Token Identifier { get; set; }
    public List<Token> GenericTypes { get; set; } = [];
    public AstNodeCLassScope? ClassScope { get; set; }
    public void SetGenericTypes(List<Token> tokens)
    {
        GenericTypes = tokens;
    }
}