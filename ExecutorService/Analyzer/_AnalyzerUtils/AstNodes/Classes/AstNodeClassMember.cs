using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

public class AstNodeClassMember
{
    public AstNodeCLassScope? OwnerClassScope { get; set; }

    public OneOf<AstNodeClassMemberFunc, AstNodeClassMemberVar, AstNodeClass> ClassMember { get; set; }
}