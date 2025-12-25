using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;

public class StatementParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : ParserCore(tokens, filePosition, symbolTableBuilder), IStatementParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;
    public AstNodeStatementScope ParseStatementScope()
    {
        _symbolTableBuilder.EnterScope();
        AstNodeStatementScope scope = new()
        {
            OwnScope = _symbolTableBuilder.CurrentScope,
            ScopeBeginOffset = ConsumeIfOfType("'{'", TokenType.OpenCurly).FilePos //consume '{' token
        };

        AstNodeStatement? scopedStatement;
        while (PeekToken() != null && (scopedStatement = ParseStatement()) != null)
        {
            scope.ScopedStatements.Add(scopedStatement);
        }
        
        scope.ScopeEndOffset = ConsumeIfOfType("'}'", TokenType.CloseCurly).FilePos; //consume '}' token
        _symbolTableBuilder.ExitScope();
        return scope;
    }
    
    public AstNodeStatement? ParseStatement()
    {
        try
        {
            var variableParser = new ScopeVariableParser(_tokens, _filePosition, _symbolTableBuilder);
            var huh = variableParser.ParseScopeMemberVariableDeclaration([MemberModifier.Final]);
            return new AstNodeStatement
            {
                Variant = new AstNodeStatementVariableDeclaration
                {
                    Variable = huh
                }
            };
        }
        catch (Exception e)
        {
            // ignored
        }

        return PeekToken()?.Type switch
        {
            TokenType.OpenCurly => ParseScopeWrapper(),
            TokenType.CloseCurly => null,
            _ => ParseDefaultStat()
        };
    }
    
    public AstNodeStatement ParseScopeWrapper()
    {
        return new AstNodeStatement
        {
            Variant = ParseStatementScope()
        };
    }
    
    public AstNodeStatement ParseDefaultStat()
    {
        return new AstNodeStatement
        {
            Variant = new AstNodeStatementUnknown(ConsumeToken())
        };
    }
}