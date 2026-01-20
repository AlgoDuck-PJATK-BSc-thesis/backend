using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClass : BaseType<AstNodeClass>, IGenerifiable, IAnnotable
{
    public List<MemberModifier> ClassModifiers { get; set; } = [];
    public List<GenericTypeDeclaration> GenericTypes { get; set; } = [];
    // public AstNodeTypeScope<AstNodeClass>? TypeScope { get; set; }
    public ComplexTypeDeclaration? Extends { get; set; }
    public List<ComplexTypeDeclaration> Implements { get; set; } = [];
    public bool IsAbstract { get; set; } = false;
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