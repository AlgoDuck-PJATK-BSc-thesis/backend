using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using ExecutorService.Errors.Exceptions;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;


public enum BinaryOperator
{
    Mul, Plus, Minus, Div, LBitShift, RBitShift, UrBitShift, Le, Ge, Eq, Neq, Lt, Gt, BitAnd, BitXor, BitOr, LogAnd, LogOr
}
public enum UnaryOperator
{
    Increment, Decrement, Tilde, Negation, Plus, Minus
}
public class ExpressionParser(List<Token> tokens, FilePosition filePosition) : ParserCore(tokens, filePosition), IExpressionParser
{
    private readonly Dictionary<TokenType, int> _operatorPrecedences = new()
    {
        [TokenType.Mul] = 10,
        [TokenType.Div] = 10,
        [TokenType.Plus] = 9,
        [TokenType.Minus] = 9,
        [TokenType.LBitShift] = 8,
        [TokenType.RBitShift] = 8,
        [TokenType.UrBitShift] = 8,
        [TokenType.CloseChevron] = 7,
        [TokenType.OpenChevron] = 7,
        [TokenType.Le] = 7,
        [TokenType.Ge] = 7,
        [TokenType.Eq] = 6,
        [TokenType.Neq] = 6,
        [TokenType.BitAnd] = 5,
        [TokenType.BitXor] = 4,
        [TokenType.BitOr] = 3,
        [TokenType.LogAnd] = 2,
        [TokenType.LogOr] = 1,
    };

    private NodeTerm ParseTerm()
    {
        if (PeekToken() == null) throw new JavaSyntaxException("");
        var nodeTerm = new NodeTerm();
        if (PeekToken()!.Type == TokenType.OpenParen)
        {
            var binaryExpression = ParseBinExpr();
            nodeTerm.Val = binaryExpression;
        }
        else if (PeekToken()!.Type == TokenType.Ident)
        {
            var variableIdentifier  = ConsumeToken();
            
            if (CheckTokenType(TokenType.OpenBrace))
            {
                ConsumeToken(); // consume '['
                var arrIndex = ParseExpr();
                nodeTerm = new NodeTerm { Val = new NodeTermArrayReference { ArrIndex = arrIndex, ArrReference = variableIdentifier } };
                ConsumeIfOfType(TokenType.CloseBrace, "]"); 
            }
            else
            {
                nodeTerm = new NodeTerm { Val = new NodeTermIdent { Identifier = variableIdentifier } };
            }
            nodeTerm = CheckPostfix(nodeTerm);
        }
        else
        {
            var numericLiteral = PeekToken()!.Type switch
            {
                TokenType.LongLit => ConsumeToken(),
                TokenType.IntLit => ConsumeToken(),
                TokenType.FloatLit => ConsumeToken(),
                TokenType.DoubleLit => ConsumeToken(),
                TokenType.CharLit => ConsumeToken(),
                TokenType.StringLit => ConsumeToken(),
                TokenType.BooleanLit => ConsumeToken(),
                _ => throw new JavaSyntaxException("unexpected token"),
            };
            nodeTerm.Val = new NodeTermLit { TermLit = numericLiteral };
        }

        return nodeTerm;
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
        var lhs = new NodeExpr { Expr = CheckUnaryPreTermParse() };
        while (true)
        {
            var curToken = PeekToken();
            if (curToken == null) break;
            var op = MapTokensToBinaryOperators(curToken);

            if (op == null) return lhs;
            ConsumeToken();

            var currentPrecedence = _operatorPrecedences[curToken.Type];
            if (currentPrecedence < minPrecedence) break;
            var nextMinPrecedence = currentPrecedence + 1;

            var rhs = ParseExpr(nextMinPrecedence);
            var binExpr = new NodeBinExpr
            {
                Lhs = lhs, Rhs = rhs, Operator = (BinaryOperator) op
            };
            lhs = new NodeExpr { Expr = binExpr };
        }

        return lhs;
    }
    
    private NodeTerm CheckPostfix(NodeTerm term)
    {
        if (CheckTokenType(TokenType.Decrement, 1) && (term.Val.IsT2 || term.Val.IsT3))
        {
            term = CreateWrappedPostfixExpression(term, BinaryOperator.Minus);
        }else if (CheckTokenType(TokenType.Increment, 1) && (term.Val.IsT2 || term.Val.IsT3))
        {
            term = CreateWrappedPostfixExpression(term, BinaryOperator.Plus);
        }

        return term;
    }


    private NodeTerm CheckUnaryPreTermParse()
    {
        if (CheckTokenType(TokenType.Minus))
        {
            ConsumeToken();
            return CreateWrappedUnaryExpression(UnaryOperator.Minus);
        }
        if (CheckTokenType(TokenType.Increment))
        {
            ConsumeToken();
            return CreateWrappedUnaryExpression(UnaryOperator.Increment);
        }

        if (CheckTokenType(TokenType.Decrement))
        {
            ConsumeToken();
            return CreateWrappedUnaryExpression(UnaryOperator.Decrement);
        }

        if (CheckTokenType(TokenType.Negation)) // TODO implement this and add TokenType.Tilde
        {
            
        }

        if (CheckTokenType(TokenType.Plus))
        {
            ConsumeToken();
        }

        return ParseTerm();
    }

    private NodeTerm CreateWrappedUnaryExpression(UnaryOperator op) // shorthand for operator which is a restricted keyword. Stating the obvious I guess
    {
            return new NodeTerm {Val = new NodeTermParen {Expr = new NodeExpr {Expr = new NodeUnaryExpr { Rhs = new NodeExpr {Expr = new NodeTerm { Val = new NodeTermParen {Expr = ParseExpr() } } }, Operator = op } } } };
    }

    private NodeTerm CreateWrappedPostfixExpression(NodeTerm term, BinaryOperator op)
    {
        return new NodeTerm {Val =  new NodeTermParen { Expr = new NodeExpr { Expr = new NodeBinExpr { Lhs = new NodeExpr {Expr = term}, Operator = BinaryOperator.Minus, Rhs = new NodeExpr {Expr = new NodeTerm {Val = new NodeTermLit { TermLit = new Token(TokenType.IntLit, "1") } } } } } }};
    }
    
    private static BinaryOperator? MapTokensToBinaryOperators(Token token)
    {
        return token.Type switch
        {
            TokenType.Mul => BinaryOperator.Mul,
            TokenType.Plus => BinaryOperator.Plus,
            TokenType.Minus => BinaryOperator.Minus,
            TokenType.Div => BinaryOperator.Div,
            TokenType.LBitShift => BinaryOperator.LBitShift,
            TokenType.RBitShift => BinaryOperator.RBitShift,
            TokenType.UrBitShift => BinaryOperator.UrBitShift,
            TokenType.Le => BinaryOperator.Le,
            TokenType.Ge => BinaryOperator.Ge,
            TokenType.Eq => BinaryOperator.Eq,
            TokenType.Neq => BinaryOperator.Neq,
            TokenType.BitAnd => BinaryOperator.BitAnd,
            TokenType.BitXor => BinaryOperator.BitXor,
            TokenType.BitOr => BinaryOperator.BitOr,
            TokenType.LogAnd => BinaryOperator.LogAnd,
            TokenType.LogOr => BinaryOperator.LogOr,
            TokenType.OpenChevron => BinaryOperator.Lt,
            TokenType.CloseChevron => BinaryOperator.Gt,
            _ => null,
        };
    }
}


public class NodeBinExpr
{
    public NodeExpr? Lhs { get; set; }
    public NodeExpr? Rhs { get; set; }
    public BinaryOperator Operator { get; set; }
}

public class NodeUnaryExpr
{
    public NodeExpr? Rhs { get; set; }
    public UnaryOperator Operator { get; set; }
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

public class NodeTermArrayReference
{
    public Token? ArrReference { get; set; }
    public NodeExpr? ArrIndex { get; set; }
}

public class NodeTerm
{
    public OneOf<NodeTermLit, NodeTermParen, NodeTermIdent, NodeTermArrayReference> Val { get; set; }
}

public class NodeExpr
{
    public OneOf<NodeBinExpr, NodeUnaryExpr, NodeTerm> Expr { get; set; }
}
