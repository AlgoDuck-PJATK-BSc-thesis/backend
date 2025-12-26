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
    public AnnotationAstNode ParseAnnotation()
    {
        ConsumeIfOfType("@", TokenType.At);
        var annotationName = ConsumeIfOfType("identifier", TokenType.Ident);
        var annotationValues = new List<AnnotationValue>();

        if (!TryConsumeTokenOfType(TokenType.OpenParen, out _) || TryConsumeTokenOfType(TokenType.CloseParen, out _))
        {
            return new AnnotationAstNode
            {
                Name = annotationName,
            };
        }

        do
        {
            if (TryConsumeTokenOfType(TokenType.Ident, out var valueName))
            {
                ConsumeIfOfType("=", TokenType.Eq);
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
    }

    private AnnotationConstExpr ParseAnnotationConstExpr()
    {
        return PeekToken()?.Type switch
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
        
    }

    private AnnotationConstArrExpr ParseArrExpr()
    {
        ConsumeToken(); // parse '{'
        if (TryConsumeTokenOfType(TokenType.CloseCurly, out _))
        {
            return new AnnotationConstArrExpr();
        }
        
        List<AnnotationConstExpr> expressions = [ParseAnnotationConstExpr()];
        while (TryConsumeTokenOfType(TokenType.Comma, out _))
        {
            if (PeekToken()?.Type == TokenType.CloseCurly) break;
            
            expressions.Add(ParseAnnotationConstExpr());
        }
        ConsumeIfOfType("}", TokenType.CloseCurly);
        return new AnnotationConstArrExpr
        {
            Expressions = expressions
        };
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
};
