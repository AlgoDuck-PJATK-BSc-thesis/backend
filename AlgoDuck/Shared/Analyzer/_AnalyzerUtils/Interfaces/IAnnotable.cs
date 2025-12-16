using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

public interface IAnnotable
{
    public void SetAnnotations(ICollection<AnnotationAstNode> annotations);
    public ICollection<AnnotationAstNode> GetAnnotations();
    public void AddAnnotation(AnnotationAstNode annotation);
}