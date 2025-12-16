using OneOf;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;

public class AstNodeStatement
{
    public OneOf<AstNodeStatementScope, AstNodeStatementUnknown, AstNodeStatementVariableDeclaration> Variant { get; set; }
}

public class AstNodeStatementVariableDeclaration
{
    public required AstNodeScopeMemberVar Variable { get; set; }
}