using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using ExecutorService.Errors.Exceptions;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class ExpressionParser(List<Token> tokens, FilePosition filePosition) : ParserCore(tokens, filePosition), IExpressionParser
{
    private readonly Dictionary<TokenType, int> _operatorPrecedences = new()
    {
        [TokenType.Mul] = 2,
        [TokenType.Div] = 2,
        [TokenType.Plus] = 1,
        [TokenType.Minus] = 1
    };

    private readonly HashSet<TokenType> _binaryOperators = [TokenType.Mul, TokenType.Plus, TokenType.Minus, TokenType.Div];
    
    public NodeTerm ParseTerm()
    {
        if (PeekToken() == null) throw new JavaSyntaxException("");
        return PeekToken()!.Type switch
        {
            TokenType.OpenParen => new NodeTerm{ Val = ParseBinExpr() },
            TokenType.Ident => new NodeTerm{ Val = new NodeTermIdent{ Identifier = ConsumeToken() } },
            TokenType.IntLit => new NodeTerm{ Val = new NodeTermLit{ TermLit = ConsumeToken() } },
            TokenType.LongLit => new NodeTerm{ Val = new NodeTermLit{ TermLit = ConsumeToken() } },
            TokenType.FloatLit => new NodeTerm{ Val = new NodeTermLit{ TermLit = ConsumeToken() } },
            TokenType.DoubleLit => new NodeTerm{ Val = new NodeTermLit{ TermLit = ConsumeToken() } },
            TokenType.CharLit => new NodeTerm{ Val = new NodeTermLit{ TermLit = ConsumeToken() } },
            TokenType.StringLit => new NodeTerm{ Val = new NodeTermLit{ TermLit = ConsumeToken() } },
            TokenType.BooleanLit => new NodeTerm{ Val = new NodeTermLit{ TermLit = ConsumeToken() } },
            _ => throw new JavaSyntaxException("unexpected token"),
        };
    }

    private NodeTermParen ParseBinExpr()
    {
        ConsumeIfOfType(TokenType.OpenParen, "unexpected token");
        var nodeExpr = ParseExpr();
        ConsumeIfOfType(TokenType.CloseParen, "Unclosed (");
        return new NodeTermParen
        {
            Expr = nodeExpr
        };
    }
    
    public NodeExpr ParseExpr(int minPrecedence = 1)
    {
        var lhs = new NodeExpr { Expr = ParseTerm() };
        while (true)
        {
            var curToken = PeekToken();
            if (curToken == null) break;
            
            if (!_binaryOperators.Contains(curToken.Type)) return lhs;
            var currentPrecedence = _operatorPrecedences[curToken.Type];
            if (currentPrecedence < minPrecedence) break;
            var nextMinPrecedence = currentPrecedence + 1;
            ConsumeToken();
            var rhs = ParseExpr(nextMinPrecedence);
            var binExpr = new NodeBinExpr
            {
                Lhs = lhs, Rhs = rhs, Operator = curToken.Type
            };
            lhs = new NodeExpr { Expr = binExpr };
        }

        return lhs;
    }
    
    
    
    /*
    TODO this is mainly for live testing purposes. Delete
    ================| start |================
    */
        
        
    public static double EvaluateExpr(NodeExpr expr)
    {
        return expr.Expr.Match(
            binExpr => ComputeOp(binExpr.Operator, EvaluateExpr(binExpr.Lhs!), EvaluateExpr(binExpr.Rhs!)),
            term => EvaluateTerm(term)
        );
    }

    private static double EvaluateTerm(NodeTerm term)
    {
        return term.Val.Match(
            intLit =>
            {
                try
                {
                    return (double) int.Parse(intLit.TermLit!.Value!.ToString());
                }
                catch (FormatException)
                {
                    return double.Parse(intLit.TermLit!.Value!.ToString());
                }
            },
            paren => EvaluateExpr(paren.Expr!),
            iden => 1
        );
    }
    private static double ComputeOp(TokenType type, double lhs, double rhs)
    {
        return type switch
        {
            TokenType.Mul => lhs * rhs,
            TokenType.Plus => lhs + rhs,
            TokenType.Minus => lhs - rhs,
            TokenType.Div => lhs / rhs,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
     /*
     ================| end |================
     */
    
    
}


public class NodeBinExpr
{
    public NodeExpr? Lhs { get; set; }
    public NodeExpr? Rhs { get; set; }
    public TokenType Operator { get; set; }
}

public class NodeTermIdent
{
    public Token? Identifier { get; set; }
}
public class NodeTermLit
{
    public Token? TermLit { get;set; }
}

public class NodeTermParen
{
    public NodeExpr? Expr { get; set; }
}

public class NodeTerm
{
    public OneOf<NodeTermLit, NodeTermParen, NodeTermIdent> Val { get; set; }
}

public class NodeExpr
{
    public OneOf<NodeBinExpr, NodeTerm> Expr { get; set; }
}
