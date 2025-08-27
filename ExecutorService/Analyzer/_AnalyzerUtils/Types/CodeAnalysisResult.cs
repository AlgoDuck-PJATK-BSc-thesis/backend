using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;

namespace ExecutorService.Analyzer._AnalyzerUtils;

public class CodeAnalysisResult(MainMethod? mainMethod, string mainClassname, bool passedValidation)
{
    public MainMethod? MainMethodIndices => mainMethod;
    public string MainClassName => mainClassname;
    public bool PassedValidation => passedValidation;
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