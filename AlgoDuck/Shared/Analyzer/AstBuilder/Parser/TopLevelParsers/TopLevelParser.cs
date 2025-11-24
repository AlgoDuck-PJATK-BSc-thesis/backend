using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers;

public class TopLevelParser(List<Token> tokens, FilePosition filePosition) :
    HighLevelParser(tokens, filePosition), ICompilationUnitParser
{
    private readonly CompilationUnitParser _compilationUnitParser = new(tokens, filePosition);
    
    public AstNodeCompilationUnit ParseCompilationUnit()
    {
        return _compilationUnitParser.ParseCompilationUnit();
    }
}