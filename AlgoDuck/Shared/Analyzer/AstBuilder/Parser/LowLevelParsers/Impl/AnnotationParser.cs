using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class AnnotationValue
{
    public Token? ValueName { get; set; }
    public required AnnotationConstExpr ConstExpr { get; set; }
}

public class AnnotationAstNode
{
    public required Token Name { get; set; }
    public List<AnnotationValue> Values { get; set; } = [];
}

public interface IAnnotationParser
{
    public AnnotationAstNode ParseAnnotation();
}

public class AnnotationParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    ParserCore(tokens, filePosition, symbolTableBuilder),
    IAnnotationParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    private const int MaxAnnotationValues = 100;
    private const int MaxArrayElements = 1000;

    public AnnotationAstNode ParseAnnotation()
    {
        return WithRecursionLimit(() =>
        {
            symbolTableBuilder.IncrementParseOps();

            ConsumeIfOfType("@", TokenType.At);
            var annotationName = ConsumeIfOfType("identifier", TokenType.Ident);
            var annotationValues = new List<AnnotationValue>();

            if (!TryConsumeTokenOfType(TokenType.OpenParen, out _) ||
                TryConsumeTokenOfType(TokenType.CloseParen, out _))
            {
                return new AnnotationAstNode
                {
                    Name = annotationName,
                };
            }

            var valueCount = 0;
            do
            {
                if (++valueCount > MaxAnnotationValues)
                {
                    throw new JavaSyntaxException($"Annotation values exceed maximum of {MaxAnnotationValues}");
                }

                Token? valueName = null;
                
                if (CheckTokenType(TokenType.Ident) && CheckTokenType(TokenType.Assign, 1))
                {
                    valueName = ConsumeToken(); // consume name
                    ConsumeToken(); // consume =
                }

                annotationValues.Add(new AnnotationValue
                {
                    ValueName = valueName,
                    ConstExpr = ParseAnnotationConstExpr()
                });
            } while (TryConsumeTokenOfType(TokenType.Comma, out _));

            ConsumeIfOfType(")", TokenType.CloseParen);

            return new AnnotationAstNode
            {
                Name = annotationName,
                Values = annotationValues
            };
        });
    }

    private AnnotationConstExpr ParseAnnotationConstExpr()
    {
        return WithRecursionLimit(() =>
        {
            if (PeekToken() == null)
            {
                throw new JavaSyntaxException("Unexpected end of input in annotation expression");
            }
            AnnotationConstExpr result = PeekToken()?.Type switch
            {
                TokenType.At => new AnnotationConstAnnotationExpr
                {
                    Annotation = ParseAnnotation()
                },
                TokenType.OpenCurly => ParseArrExpr(),
                _ => new AnnotationConstBinExpr
                {
                    Expr = new ExpressionParser(_tokens, _filePosition, _symbolTableBuilder).ParseExpr()
                }
            };
            return result;
        });
    }

    private AnnotationConstArrExpr ParseArrExpr()
    {
        return WithRecursionLimit(() =>
        {
            ConsumeToken(); // consume '{'
            
            if (TryConsumeTokenOfType(TokenType.CloseCurly, out _))
            {
                return new AnnotationConstArrExpr();
            }

            var elementCount = 0;
            List<AnnotationConstExpr> expressions = [];

            do
            {
                if (++elementCount > MaxArrayElements)
                {
                    throw new JavaSyntaxException($"Annotation array exceeds maximum of {MaxArrayElements} elements");
                }

                if (PeekToken()?.Type == TokenType.CloseCurly)
                {
                    break;
                }

                expressions.Add(ParseAnnotationConstExpr());
                
            } while (TryConsumeTokenOfType(TokenType.Comma, out _));

            ConsumeIfOfType("}", TokenType.CloseCurly);
            
            return new AnnotationConstArrExpr
            {
                Expressions = expressions
            };
        });
    }
}

public abstract class AnnotationConstExpr;

public class AnnotationConstBinExpr : AnnotationConstExpr
{
    public required NodeExpr Expr { get; set; }
}

public class AnnotationConstArrExpr : AnnotationConstExpr
{
    public List<AnnotationConstExpr> Expressions { get; set; } = [];
}

public class AnnotationConstAnnotationExpr : AnnotationConstExpr
{
    public required AnnotationAstNode Annotation { get; set; }
}