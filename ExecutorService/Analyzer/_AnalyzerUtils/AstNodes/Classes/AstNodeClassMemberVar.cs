using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClassMemberVar
{
    public AstNodeClassMember? ClassMember { get; set; }
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public AstNodeScopeMemberVar ScopeMemberVar { get; set; } = new AstNodeScopeMemberVar();
}