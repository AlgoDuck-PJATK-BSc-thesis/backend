using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.Types;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Interfaces;

public class AstNodeInterface : IType<AstNodeInterface>, IGenericSettable
{
    public AccessModifier InterfaceAccessModifier { get; set; } = AccessModifier.Default;
    public Token? Identifier { get; set; }
    public List<MemberModifier> Modifiers { get; set; } = [];
    public List<GenericTypeDeclaration> GenericTypes { get; set; } = [];
    public AstNodeTypeScope<AstNodeInterface>? InterfaceScope { get; set; }
    public List<ComplexTypeDeclaration> Extends { get; set; } = [];
    public Token? GetIdentifier()
    {
        return Identifier;
    }

    public List<AstNodeTypeMember<AstNodeInterface>> GetMembers()
    {
        return InterfaceScope!.TypeMembers;
    }

    public AstNodeTypeScope<AstNodeInterface>? GetScope()
    {
        return InterfaceScope;
    }

    public void SetGenericTypes(List<GenericTypeDeclaration> tokens)
    {
        GenericTypes = tokens;
    }
}