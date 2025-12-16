using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodeProgram
{
    public List<AstNodeCompilationUnit> ProgramCompilationUnits { get; set; } = [];
    public required SymbolTableBuilder SymbolTableBuilder { get; set; }
}