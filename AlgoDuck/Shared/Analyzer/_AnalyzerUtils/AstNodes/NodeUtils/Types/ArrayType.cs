using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using OneOf;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;

public class ArrayType
{
    public OneOf<MemberType, ArrayType, ComplexTypeDeclaration> BaseType { get; set; }
    public int Dim { get; set; }
    public bool IsVarArgs { get; set; } = false;
    public override string ToString()
    {
        var baseType = BaseType.Match(
            t0 => Enum.GetName(t0),
            t1 => t1.ToString(),
            t2 => t2.ToString()) ?? "err";
        
        return $"{baseType}{string.Concat(Enumerable.Repeat("[]", Dim))}";
    }
}