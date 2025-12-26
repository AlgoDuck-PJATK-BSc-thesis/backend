using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeMemberVar<T> : ITypeMember<T>, IAnnotable where T : BaseType<T>
{
    private T? Owner { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Default;
    public AstNodeScopeMemberVar ScopeMemberVar { get; set; } = new();
    private ICollection<AnnotationAstNode> Annotations { get; set; } = [];
    public required Scope DeclaredScope { get; set; }

    public T? GetMemberType()
    {
        return Owner;
    }

    public void SetMemberType(T t)
    {
        Owner = t;
    }

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