using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers;

public class TopLevelParser : HighLevelParser, ICompilationUnitParser
{
    private readonly CompilationUnitParser _compilationUnitParser;

    public TopLevelParser(List<Token> tokens, SymbolTableBuilder symbolTableBuilder) : base(tokens, FilePosition.GetFilePosition(out var filePosition), symbolTableBuilder)
    {
        _compilationUnitParser = new CompilationUnitParser(tokens, filePosition, symbolTableBuilder);
    }

    public AstNodeCompilationUnit ParseCompilationUnit()
    {
        return _compilationUnitParser.ParseCompilationUnit();
    }
}