using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

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
        var symbolTableBuild = new SymbolTableBuilder();
        foreach (var compilationUnit in compilationUnits)
        {
            var compilationUnitParser = new TopLevelParser(compilationUnit, symbolTableBuild);
            program.ProgramCompilationUnits.Add(compilationUnitParser.ParseCompilationUnit());
        }

        return program;
    }

}