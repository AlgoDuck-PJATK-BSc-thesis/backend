using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

namespace ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

public class AstNodeEnum  : BaseType<AstNodeEnum>, IAnnotable
{
    public ICollection<AnnotationAstNode> Annotations { get; set; } = [];
    public ICollection<MemberModifier> Modifiers { get; set; } = [];
    // public AstNodeTypeScope<AstNodeEnum>? TypeScope { get; set; }


    public void SetAnnotations(ICollection<AnnotationAstNode> annotations)
    {
        Annotations = annotations;
    }

    public ICollection<AnnotationAstNode> GetAnnotations() => Annotations;

    public void AddAnnotation(AnnotationAstNode annotation)
    {
        Annotations.Add(annotation);
    }
}