using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;

public class HighLevelParser(List<Token> tokens, FilePosition filePosition) : MidLevelParser(tokens, filePosition)
{
    
}