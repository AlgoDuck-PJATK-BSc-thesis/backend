using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using OneOf;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

public class ArrayType
{
    public OneOf<MemberType> BaseType { get; set; }
    public int Dim { get; set; }
}