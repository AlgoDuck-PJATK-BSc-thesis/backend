using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;

public class HighLevelParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : MidLevelParser(tokens, filePosition, symbolTableBuilder)
{
    
}