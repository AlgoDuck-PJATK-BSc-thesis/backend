using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;

public class AstNodeScopeMemberVar
{
    public List<MemberModifier> VarModifiers { get; set; } = [];
    public OneOf<MemberType, ArrayType, ComplexTypeDeclaration> Type { get; set; }
    public Token? Identifier { get; set; }
    public NodeExpr? VariableValue { get; set; }
}