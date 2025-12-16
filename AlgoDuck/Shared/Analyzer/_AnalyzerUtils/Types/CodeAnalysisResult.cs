using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

public class CodeAnalysisResult
{
    public required AstNodeMemberFunc<AstNodeClass> Main { get; set; }
    public required MainMethod? MainMethodIndices { get; set; }
    public required string MainClassName { get; set; }
    public required bool PassedValidation { get; set; }
}

public class MainMethod(int begin, int end)
{
    public int MethodFileBeginIndex { get; set; } = begin;
    public int MethodFileEndIndex { get; set; } = end;

    public static MainMethod? MakeFromAstNodeMain(AstNodeMemberFunc<AstNodeClass>? main)
    {
        if (main == null)
        {
            return null;
        }
        
        return new MainMethod(main.FuncScope!.ScopeBeginOffset, main.FuncScope.ScopeEndOffset);
    }
}