using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;

public class HighLevelParser(List<Token> tokens, FilePosition filePosition) : MidLevelParser(tokens, filePosition)
{
    
}