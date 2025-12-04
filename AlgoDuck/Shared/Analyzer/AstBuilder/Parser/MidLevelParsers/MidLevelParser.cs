using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers;

public class MidLevelParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    LowLevelParser(tokens, filePosition, symbolTableBuilder), 
    IScopeVariableParser, IStatementParser
{
    private readonly ScopeVariableParser _scopeVariableParser = new(tokens, filePosition, symbolTableBuilder);
    private readonly StatementParser _statementParser = new(tokens, filePosition, symbolTableBuilder);
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers)
    {
        return _scopeVariableParser.ParseScopeMemberVariableDeclaration(permittedModifiers);
    }
        
    public AstNodeStatementScope ParseStatementScope()
    {
        _symbolTableBuilder.EnterScope();
        var astNodeStatementScope = _statementParser.ParseStatementScope();
        _symbolTableBuilder.ExitScope();
        return astNodeStatementScope;
    }

    public AstNodeStatement? ParseStatement()
    {
        return _statementParser.ParseStatement();
    }

    public AstNodeStatement ParseDefaultStat()
    {
        return _statementParser.ParseDefaultStat();
    }

    public AstNodeStatement ParseScopeWrapper()
    {
        return _statementParser.ParseScopeWrapper();
    }
}