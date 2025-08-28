using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers;

public class TopLevelParser(List<Token> tokens, FilePosition filePosition) :
    HighLevelParser(tokens, filePosition), ICompilationUnitParser
{
    private readonly CompilationUnitParser _compilationUnitParser = new(tokens, filePosition);
    
    public AstNodeCompilationUnit ParseCompilationUnit()
    {
        return _compilationUnitParser.ParseCompilationUnit();
    }
}