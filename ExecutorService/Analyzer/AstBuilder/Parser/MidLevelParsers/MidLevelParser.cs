using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;

namespace ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers;

public class MidLevelParser(List<Token> tokens, FilePosition filePosition) :
    LowLevelParser(tokens, filePosition), 
    IScopeVariableParser, IStatementParser
{
    private readonly ScopeVariableParser _scopeVariableParser = new(tokens, filePosition);
    private readonly StatementParser _statementParser = new(tokens, filePosition);

    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers)
    {
        return _scopeVariableParser.ParseScopeMemberVariableDeclaration(permittedModifiers);
    }
        
    public AstNodeStatementScope ParseStatementScope()
    {
        return _statementParser.ParseStatementScope();
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