using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using OneOf;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;

public class AstNodeScopeMemberVar
{
    public List<MemberModifier> VarModifiers { get; set; } = [];
    public OneOf<MemberType, ArrayType, ComplexTypeDeclaration> Type { get; set; }
    public Token? Identifier { get; set; }
    public NodeExpr? VariableValue { get; set; }
}