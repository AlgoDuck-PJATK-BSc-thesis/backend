using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class CompilationUnitParser(List<Token> tokens, FilePosition filePosition) : HighLevelParser(tokens, filePosition)
{
    private readonly FilePosition _filePosition = filePosition;
    private readonly List<Token> _tokens = tokens;

    public AstNodeCompilationUnit ParseCompilationUnit()
    {
        var compilationUnit = new AstNodeCompilationUnit();

        if (CheckTokenType(TokenType.Package))
        {
            ConsumeIfOfType(TokenType.Package, "not gonna happen, put here for readability");
            AstNodePackage package = new();
            new TopLevelStatementParser(_tokens, _filePosition).ParseImportsAndPackages(package);
            compilationUnit.Package = package;
        }

        while (CheckTokenType(TokenType.Import))
        {
            ConsumeIfOfType(TokenType.Import, "not gonna happen, put here for readability");
            AstNodeImport import = new();
            new TopLevelStatementParser(_tokens, _filePosition).ParseImportsAndPackages(import);
            compilationUnit.Imports.Add(import);
        }

        while (PeekToken() != null)
        {
        
            compilationUnit.CompilationUnitTopLevelStatements.Add(new TopLevelStatementParser(_tokens, _filePosition).ParseTypeDefinition());
        }

        return compilationUnit;
    }
}