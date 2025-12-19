using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;
using OneOf;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

public class AstNodeMemberFunc<T> : IGenerifiable, IAnnotable, ITypeMember<T> where T : BaseType<T>
{
    public T? Owner { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Default;
    public List<MemberModifier> Modifiers { get; set; } = [];
    public List<GenericTypeDeclaration> GenericTypes { get; set; } = [];
    public OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration>? FuncReturnType { get; set; } // same here
    public Token? Identifier { get; set; }
    public List<AstNodeScopeMemberVar> FuncArgs { get; set; } = [];
    public AstNodeStatementScope? FuncScope { get; set; }
    public bool IsConstructor { get; set; } = false;
    public List<Token> ThrownExceptions { get; set; } = [];
    public ICollection<AnnotationAstNode> Annotations { get; set; } = [];

    public void SetGenericTypes(List<GenericTypeDeclaration> tokens)
    {
        GenericTypes = tokens;
    }

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