using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers;

namespace ExecutorService.Analyzer.AstBuilder.Parser;

public interface IParser
{
    public AstNodeProgram ParseProgram(List<List<Token>> compilationUnits);
}

public class ParserSimple : IParser
{
    public AstNodeProgram ParseProgram(List<List<Token>> compilationUnits)
    {
        AstNodeProgram program = new();
        foreach (var compilationUnit in compilationUnits)
        {
            var compilationUnitParser = new TopLevelParser(compilationUnit, new FilePosition());
            program.ProgramCompilationUnits.Add(compilationUnitParser.ParseCompilationUnit());
        }

        return program;
    }

}