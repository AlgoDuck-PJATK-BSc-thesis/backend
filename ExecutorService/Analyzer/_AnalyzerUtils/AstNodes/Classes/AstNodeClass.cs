using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClass
{
    public AccessModifier ClassAccessModifier { get; set; } = AccessModifier.Private;
    public List<MemberModifier> ClassModifiers { get; set; } = new();
    public Token Identifier { get; set; }
    public AstNodeCLassScope? ClassScope { get; set; }
}