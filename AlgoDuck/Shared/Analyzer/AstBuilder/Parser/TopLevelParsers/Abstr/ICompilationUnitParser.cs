using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface ICompilationUnitParser
{
    public AstNodeCompilationUnit ParseCompilationUnit();

}