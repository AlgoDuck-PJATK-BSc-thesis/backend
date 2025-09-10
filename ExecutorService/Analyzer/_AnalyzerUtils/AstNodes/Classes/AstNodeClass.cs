using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.Types;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClass : IGenericSettable, IType<AstNodeClass>
{
    public AccessModifier ClassAccessModifier { get; set; } = AccessModifier.Default;
    public List<MemberModifier> ClassModifiers { get; set; } = [];
    public Token? Identifier { get; set; }
    public List<GenericTypeDeclaration> GenericTypes { get; set; } = [];
    public AstNodeTypeScope<AstNodeClass>? ClassScope { get; set; }
    public ComplexTypeDeclaration? Extends { get; set; }
    public List<ComplexTypeDeclaration> Implements { get; set; } = [];
    public bool IsAbstract { get; set; } = false;

    public void SetGenericTypes(List<GenericTypeDeclaration> tokens)
    {
        GenericTypes = tokens;
    }

    public Token? GetIdentifier()
    {
        return Identifier;
    }

    public List<AstNodeTypeMember<AstNodeClass>> GetMembers()
    {
        return ClassScope!.TypeMembers;
    }

    public AstNodeTypeScope<AstNodeClass>? GetScope()
    {
        return ClassScope;
    }
}