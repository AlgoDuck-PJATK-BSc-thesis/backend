using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClass : IGenericSettable
{
    public AccessModifier ClassAccessModifier { get; set; } = AccessModifier.Private;
    public List<MemberModifier> ClassModifiers { get; set; } = [];
    public Token? Identifier { get; set; }
    public List<GenericTypeDeclaration> GenericTypes { get; set; } = [];
    public AstNodeCLassScope? ClassScope { get; set; }
    public Token? Extends { get; set; }
    public List<Token> Implements { get; set; } = [];
    public bool IsAbstract { get; set; } = false;

    public void SetGenericTypes(List<GenericTypeDeclaration> tokens)
    {
        GenericTypes = tokens;
    }
}