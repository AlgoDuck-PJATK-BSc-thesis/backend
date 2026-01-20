using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Interfaces;

public class AstNodeInterface : BaseType<AstNodeInterface>, IGenerifiable, IAnnotable
{
    public List<MemberModifier> Modifiers { get; set; } = [];
    public List<GenericTypeDeclaration> GenericTypes { get; set; } = [];
    // public AstNodeTypeScope<AstNodeInterface>? TypeScope { get; set; }
    public List<ComplexTypeDeclaration> Extends { get; set; } = [];
    private ICollection<AnnotationAstNode> Annotations { get; set; } = [];

    public void SetAnnotations(ICollection<AnnotationAstNode> annotations)
    {
        Annotations = annotations;
    }

    public ICollection<AnnotationAstNode> GetAnnotations() => Annotations;

    public void AddAnnotation(AnnotationAstNode annotation)
    {
        Annotations.Add(annotation);
    }


    public void SetGenericTypes(List<GenericTypeDeclaration> tokens)
    {
        GenericTypes = tokens;
    }
}
