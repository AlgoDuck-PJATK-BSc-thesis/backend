using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClassMember
{
    public OneOf<AstNodeClassMemberFunc, AstNodeClassMemberVar, AstNodeClass> ClassMember { get; set; }
}