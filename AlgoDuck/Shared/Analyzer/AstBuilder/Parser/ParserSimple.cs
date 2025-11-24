using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser;

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