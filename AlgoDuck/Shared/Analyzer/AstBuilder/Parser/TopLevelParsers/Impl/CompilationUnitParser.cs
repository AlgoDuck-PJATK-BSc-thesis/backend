using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class CompilationUnitParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : HighLevelParser(tokens, filePosition, symbolTableBuilder)
{
    private readonly FilePosition _filePosition = filePosition;
    private readonly List<Token> _tokens = tokens;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    public AstNodeCompilationUnit ParseCompilationUnit()
    {
        var compilationUnit = new AstNodeCompilationUnit();
        var topLevelStatementParser = new TopLevelStatementParser(_tokens, _filePosition, _symbolTableBuilder);
        
        if (CheckTokenType(TokenType.Package))
        {
            ConsumeIfOfType("not gonna happen, put here for readability", TokenType.Package);
            AstNodePackage package = new();
            topLevelStatementParser.ParseImportsAndPackages(package);
            compilationUnit.Package = package;
        }

        while (CheckTokenType(TokenType.Import))
        {
            ConsumeIfOfType("not gonna happen, put here for readability", TokenType.Import);
            AstNodeImport import = new();
            topLevelStatementParser.ParseImportsAndPackages(import);
            compilationUnit.Imports.Add(import);
        }

        while (PeekToken() != null)
        {
        
            compilationUnit.CompilationUnitTopLevelStatements.Add(topLevelStatementParser.ParseTypeDefinition());
        }

        return compilationUnit;
    }
}