using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
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

        if (TryConsumeTokenOfType(TokenType.Package, out var _))
        {
            try
            {
                compilationUnit.Package = new AstNodePackage();
                topLevelStatementParser.ParseImportsAndPackages(compilationUnit.Package);
            }
            catch (JavaSyntaxException)
            {
                // ignored. If syntax error in import detected forward to actual compilation for more descriptive errors.
            }
        }
        
        while (TryConsumeTokenOfType(TokenType.Import, out var _))
        {
            try
            {
                var import = new AstNodeImport();
                topLevelStatementParser.ParseImportsAndPackages(import);
                compilationUnit.Imports.Add(import);
            }
            catch (JavaSyntaxException)
            {
                // ignored. If syntax error in import detected forward to actual compilation for more descriptive errors.
            }
        }

        while (PeekToken() != null)
        {
            var currToken = PeekToken();
            try{
                compilationUnit.CompilationUnitTopLevelStatements.Add(topLevelStatementParser.ParseTypeDefinition());
            } catch (JavaSyntaxException)
            {
                if (currToken == PeekToken()) // prevent deadlocks on unprocessable code
                {
                    ConsumeToken();
                }
                // ignored. If syntax error in import detected forward to actual compilation for more descriptive errors.
            }
        }

        return compilationUnit;
    }
}