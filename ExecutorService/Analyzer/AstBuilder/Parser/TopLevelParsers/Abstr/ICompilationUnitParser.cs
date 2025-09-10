using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface ICompilationUnitParser
{
    public AstNodeCompilationUnit ParseCompilationUnit();

}