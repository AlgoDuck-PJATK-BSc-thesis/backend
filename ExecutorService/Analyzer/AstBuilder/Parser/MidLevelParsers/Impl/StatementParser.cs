using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;

namespace ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;

public class StatementParser(List<Token> tokens, FilePosition filePosition) : ParserCore(tokens, filePosition), IStatementParser
{
    public AstNodeStatementScope ParseStatementScope()
    {

        AstNodeStatementScope scope = new()
        {
            ScopeBeginOffset = ConsumeIfOfType(TokenType.OpenCurly, "'{'").FilePos //consume '{' token
        };

        AstNodeStatement? scopedStatement;
        while (PeekToken() != null && (scopedStatement = ParseStatement()) != null)
        {
            scope.ScopedStatements.Add(scopedStatement);
        }
        
        scope.ScopeEndOffset = ConsumeIfOfType(TokenType.CloseCurly, "'}'").FilePos; //consume '}' token
        
        return scope;
    }
    
    public AstNodeStatement? ParseStatement()
    {
        switch (PeekToken().Type)
        {
            case TokenType.OpenCurly:
                return ParseScopeWrapper();
            case TokenType.CloseCurly:
                return null;
            default:
                return ParseDefaultStat();
        }
    }
    
    public AstNodeStatement ParseScopeWrapper()
    {
        return new AstNodeStatement()
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