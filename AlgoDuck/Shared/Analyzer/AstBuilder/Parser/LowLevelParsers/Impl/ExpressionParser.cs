using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;


public enum BinaryOperator
{
    Mul, Plus, Minus, Div, LBitShift, RBitShift, UrBitShift, Le, Ge, Eq, Neq, Lt, Gt, BitAnd, BitXor, BitOr, LogAnd, LogOr
}
public enum UnaryOperator
{
    Increment, Decrement, Tilde, Negation, Plus, Minus
}

public enum LiteralType
{
    Long, Int, Float, Double, Char, String, Boolean, Array, Ident
}



public class ExpressionParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : ParserCore(tokens, filePosition, symbolTableBuilder), IExpressionParser
{
    
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    private const int MaxSwallowIterations = 10000;
    private const int MaxLambdaLookahead = 1000;
    private const int MaxArrayLiteralElements = 10000;
    
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

    private Token SwallowUntilExpressionEnd()
    {
        var parenDepth = 0;
        var bracketDepth = 0;
        var iterations = 0;

        Token? token = null;
        while (PeekToken() != null)
        {
            if (++iterations > MaxSwallowIterations)
            {
                throw new JavaSyntaxException("Expression too complex or malformed");
            }
            
            token = PeekToken()!;

            if (token.Type == TokenType.OpenCurly)
            {
                var statementParser = new StatementParser(_tokens, _filePosition, _symbolTableBuilder);
                statementParser.ParseStatementScope();
                continue;
            }

            switch (token.Type)
            {
                case TokenType.OpenParen: parenDepth++; break;
                case TokenType.CloseParen: parenDepth--; break;
                case TokenType.OpenBrace: bracketDepth++; break;
                case TokenType.CloseBrace: bracketDepth--; break;
            }

            if (token.Type == TokenType.Semi && parenDepth <= 0 && bracketDepth <= 0)
                break;

            if (token.Type == TokenType.Comma && parenDepth <= 0 && bracketDepth <= 0)
                break;

            if (parenDepth < 0 || bracketDepth < 0)
                break;
            
            if (token.Type == TokenType.CloseCurly)
                throw new JavaSyntaxException("unexpected '}'");

            ConsumeToken();
        }
        
        if (PeekToken() == null)
            throw new JavaSyntaxException("unexpected end of expression");

        return token ?? throw new JavaSyntaxException("unexpected end of expression");
    }

    private bool IsComplexExpressionStart()
    {
        var token = PeekToken();
        if (token == null) return false;

        return token.Type switch
        {
            TokenType.New => true,
            TokenType.Switch => true,
            _ => false
        };
    }

    private bool LooksLikeLambdaOrMethodRef()
    {
        _filePosition.CreateCheckpoint();
        try
        {
            var depth = 0;
            var iterations = 0;
            
            while (PeekToken() != null)
            {
                if (++iterations > MaxLambdaLookahead)
                {
                    return false;
                }
                
                var t = PeekToken()!;
                
                switch (t.Type)
                {
                    case TokenType.OpenParen:
                        depth++;
                        break;
                    case TokenType.CloseParen:
                        depth--;
                        break;
                }
                
                if (depth <= 0 && t.Type is TokenType.Arrow or TokenType.DoubleColon)
                    return true;
                
                if (t.Type is TokenType.Semi or TokenType.Comma)
                    return false;
                
                if (depth < 0) return false;
                
                ConsumeToken();
            }
            return false;
        }
        finally
        {
            _filePosition.LoadCheckpoint();
        }
    }

    private NodeTerm ParseTerm()
    {
        return WithRecursionLimit(() =>
        {
            if (PeekToken() == null) throw new JavaSyntaxException("Unexpected end of input while parsing term");
            
            var nodeTerm = new NodeTerm();

            if (IsComplexExpressionStart())
            {
                SwallowUntilExpressionEnd();
                return new NodeTerm { Val = new NodeTermSwallowed() };
            }

            if (PeekToken()!.Type == TokenType.OpenParen)
            {
                if (LooksLikeLambdaOrMethodRef())
                {
                    SwallowUntilExpressionEnd();
                    return new NodeTerm { Val = new NodeTermSwallowed() };
                }

                _filePosition.CreateCheckpoint();
                if (TryParseCast(out var cast))
                {
                    nodeTerm.Val = cast!;
                }
                else
                {
                    _filePosition.LoadCheckpoint();
                    var binaryExpression = ParseBinExpr();
                    nodeTerm.Val = binaryExpression;
                }
            }
            else if (PeekToken()!.Type == TokenType.Ident)
            {
                var variableIdentifier = ConsumeToken();

                if (CheckTokenType(TokenType.Arrow) || CheckTokenType(TokenType.DoubleColon) ||
                    CheckTokenType(TokenType.OpenParen) || CheckTokenType(TokenType.Dot))
                {
                    SwallowUntilExpressionEnd();
                    return new NodeTerm { Val = new NodeTermSwallowed() };
                }

                if (CheckTokenType(TokenType.OpenBrace))
                {
                    ConsumeToken(); // consume '['
                    var arrIndex = ParseExpr();
                    nodeTerm = new NodeTerm
                        { Val = new NodeTermArrayReference { ArrReference = variableIdentifier, ArrIndex = arrIndex } };
                    ConsumeIfOfType("]", TokenType.CloseBrace);
                }
                else
                {
                    nodeTerm = new NodeTerm { Val = new NodeTermIdent { Identifier = variableIdentifier } };
                }

                nodeTerm = CheckPostfix(nodeTerm);
            }
            else if (CheckTokenType(TokenType.OpenCurly))
            {
                SwallowUntilExpressionEnd();
                return new NodeTerm { Val = new NodeTermSwallowed() };
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
                    _ => SwallowUntilExpressionEnd()
                };
                nodeTerm.Val = new NodeTermLit { TermLit = numericLiteral };
            }

            return nodeTerm;
        });
    }

    private NodeTermParen ParseBinExpr()
    {
        return WithRecursionLimit(() =>
        {
            ConsumeIfOfType("unexpected token", TokenType.OpenParen);
            var nodeExpr = ParseExpr();
            ConsumeIfOfType("Unclosed (", TokenType.CloseParen);
            return new NodeTermParen
            {
                Expr = nodeExpr
            };
        });
    }
    
    private bool TryParseCast(out NodeTermCast? cast)
    {
        cast = null;
        var typeParser = new TypeParser(_tokens, _filePosition, _symbolTableBuilder);
        ConsumeIfOfType("(", TokenType.OpenParen);
        
        OneOf<MemberType, ArrayType, ComplexTypeDeclaration> castType;
        try
        {
            castType = typeParser.ParseStandardType();
        }
        catch (Exception)
        {
            return false;
        }

        if (castType.IsT2 && castType.AsT2.GenericInitializations == null)
        {
            var symbol = _symbolTableBuilder.Resolve(castType.AsT2.Identifier);
    
            if (symbol is VariableSymbol && !_symbolTableBuilder.IsType(castType.AsT2.Identifier))
            {
                return false;
            }
        }
    
        ConsumeIfOfType(")", TokenType.CloseParen);

        cast = new NodeTermCast
        {
            TargetType = castType,
            Expression = ParseExpr()
        };
        return true;
    }
    
    public NodeExpr ParseExpr(int minPrecedence = 1)
    {
        return WithRecursionLimit(() =>
        {
            symbolTableBuilder.IncrementParseOps();

            var lhs = new NodeExpr { Expr = CheckUnaryPreTermParse() };
            
            while (true)
            {
                var curToken = PeekToken();
                if (curToken == null) break;

                if (curToken.Type == TokenType.Wildcard)
                {
                    SwallowUntilExpressionEnd();
                    return new NodeExpr { Expr = new NodeTerm { Val = new NodeTermSwallowed() } };
                }

                var op = MapTokensToBinaryOperators(curToken);

                if (op == null) return lhs;

                var currentPrecedence = _operatorPrecedences[curToken.Type];
                if (currentPrecedence < minPrecedence) break;
                var nextMinPrecedence = currentPrecedence + 1;

                ConsumeToken(); // consume operator token

                var rhs = ParseExpr(nextMinPrecedence);
                var binExpr = new NodeBinExpr
                {
                    Lhs = lhs, Rhs = rhs, Operator = (BinaryOperator)op
                };
                lhs = new NodeExpr { Expr = binExpr };
            }

            return lhs;
        });
    }
    
    private NodeTerm CheckPostfix(NodeTerm term)
    {
        if (CheckTokenType(TokenType.Decrement, 1) && (term.Val.IsT2 || term.Val.IsT3))
        {
            term = CreateWrappedPostfixExpression(term, BinaryOperator.Minus);
        }
        else if (CheckTokenType(TokenType.Increment, 1) && (term.Val.IsT2 || term.Val.IsT3))
        {
            term = CreateWrappedPostfixExpression(term, BinaryOperator.Plus);
        }

        return term;
    }


    private NodeTermArrayLiteral ParseArrayLiteral()
    {
        ConsumeIfOfType("open curly brace", TokenType.OpenCurly); // consume opening '{'
        var arrayLiteral = new NodeTermArrayLiteral
        {
            ArrayType = LiteralType.Ident
        };
        
        if (PeekToken() == null) throw new JavaSyntaxException("expected tokens");
        
        var inferredType = TokenType.Ident;
        var elementCount = 0;
        
        while (PeekToken() != null && CheckTokenType(TokenType.Comma, 1))
        {
            if (++elementCount > MaxArrayLiteralElements)
            {
                throw new JavaSyntaxException($"Array literal exceeds maximum of {MaxArrayLiteralElements} elements");
            }
            
            var curToken = PeekToken()!;
            var tokenLiteralType = MapTokenTypeToLiteralType(curToken.Type);
            if (inferredType == TokenType.Ident && tokenLiteralType != null && tokenLiteralType != LiteralType.Ident)
            {
                inferredType = PeekToken()!.Type;
            }
            arrayLiteral.ArrayMembers.Add(ConsumeIfOfType($"\nType mismatch.\n\texpected: {inferredType}\n\tgot:\t{PeekToken()!.Type}", TokenType.Ident, inferredType));
            ConsumeIfOfType("expected ','", TokenType.Comma);
        }
        
        if (PeekToken() == null)
        {
            throw new JavaSyntaxException("Unexpected end of input in array literal");
        }
        
        arrayLiteral.ArrayMembers.Add(ConsumeIfOfType($"\nType mismatch.\n\texpected: {inferredType}\n\tgot:\t{PeekToken()!.Type}", TokenType.Ident, inferredType));
        ConsumeIfOfType("expected '}'", TokenType.CloseCurly);

        arrayLiteral.ArrayType = (LiteralType) MapTokenTypeToLiteralType(inferredType)!; 
        
        return arrayLiteral;
    }

    private NodeTerm CheckUnaryPreTermParse()
    {
        return WithRecursionLimit(() =>
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

            if (CheckTokenType(TokenType.Negation))
            {
                ConsumeToken();
                return CreateWrappedUnaryExpression(UnaryOperator.Negation);
            }

            if (CheckTokenType(TokenType.Plus))
            {
                ConsumeToken();
            }

            return ParseTerm();
        });
    }

    private NodeTerm CreateWrappedUnaryExpression(UnaryOperator op)
    {
        return new NodeTerm {Val = new NodeTermParen {Expr = new NodeExpr {Expr = new NodeUnaryExpr { Rhs = new NodeExpr {Expr = new NodeTerm { Val = new NodeTermParen {Expr = ParseExpr() } } }, Operator = op } } } };
    }

    private NodeTerm CreateWrappedPostfixExpression(NodeTerm term, BinaryOperator op)
    {
        return new NodeTerm {Val =  new NodeTermParen { Expr = new NodeExpr { Expr = new NodeBinExpr { Lhs = new NodeExpr {Expr = term}, Operator = op, Rhs = new NodeExpr {Expr = new NodeTerm {Val = new NodeTermLit { TermLit = new Token(TokenType.IntLit, "1") } } } } } }};
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
    
    private static LiteralType? MapTokenTypeToLiteralType(TokenType token)
    {
        return token switch
        {
            TokenType.LongLit => LiteralType.Long,
            TokenType.IntLit => LiteralType.Int,
            TokenType.FloatLit => LiteralType.Float,
            TokenType.DoubleLit => LiteralType.Double,
            TokenType.CharLit => LiteralType.Char,
            TokenType.StringLit => LiteralType.String,
            TokenType.BooleanLit => LiteralType.Boolean,
            TokenType.OpenBrace => LiteralType.Array,
            TokenType.Ident => LiteralType.Ident,
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
    public OneOf<Token, NodeTermArrayLiteral>? TermLit { get; set; }
}

public class NodeTermArrayLiteral
{
    public LiteralType ArrayType { get; set; }
    public List<Token> ArrayMembers { get; set; } = [];
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

public class NodeTermCast
{
    public OneOf<MemberType, ArrayType, ComplexTypeDeclaration> TargetType { get; set; }
    public NodeExpr? Expression { get; set; }
}

public class NodeTermSwallowed { }


public class NodeTerm
{
    public OneOf<NodeTermLit, NodeTermParen, NodeTermIdent, NodeTermArrayReference, NodeTermCast, NodeTermSwallowed> Val { get; set; }
}

public class NodeExpr
{
    public OneOf<NodeBinExpr, NodeUnaryExpr, NodeTerm> Expr { get; set; }
}
