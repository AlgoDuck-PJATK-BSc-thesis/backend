using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class TypeParser(List<Token> tokens, FilePosition filePosition) : ParserCore(tokens, filePosition), ITypeParser
{
    private static readonly HashSet<TokenType> SimpleTypes =
    [
        TokenType.Byte, TokenType.Short, TokenType.Int, TokenType.Long,
        TokenType.Float, TokenType.Double, TokenType.Char,
        TokenType.Boolean, TokenType.String
    ];
    
    public OneOf<MemberType, SpecialMemberType, ArrayType, Token>? ParseType()
    {
        var token = TryConsume();
        if (TokenIsSimpleType(token))
        {
            var type = ParseSimpleType(token);
            var dim = 0;
            if (CheckTokenType(TokenType.OpenBrace) && CheckTokenType(TokenType.CloseBrace, 1))
            {
                TryConsumeNTokens(2);
                dim++;
                while (CheckTokenType(TokenType.OpenBrace) && CheckTokenType(TokenType.CloseBrace, 1))
                {
                    dim++;
                    TryConsumeNTokens(2);
                }
                return new ArrayType()
                {
                    BaseType = type,
                    Dim = dim
                };
            }

            return type;

        }

        return token.Type switch
        {
            TokenType.Ident => token,
            TokenType.Void => SpecialMemberType.Void,
            _ => null
        };
    }

    public bool TokenIsSimpleType(Token? token)
    {
        if (token is null)
        {
            throw new JavaSyntaxException("again what are you even giving me?");
        }
        
        return SimpleTypes.Contains(token.Type);
        
    }

    public MemberType ParseSimpleType(Token token)
    {
        return token.Type switch
        {
            TokenType.Byte => MemberType.Byte,
            TokenType.Short => MemberType.Short,
            TokenType.Int => MemberType.Int,
            TokenType.Long => MemberType.Long,
            TokenType.Float => MemberType.Float,
            TokenType.Double => MemberType.Double,
            TokenType.Char => MemberType.Char,
            TokenType.Boolean => MemberType.Boolean,
            TokenType.String => MemberType.String,
            _ => throw new ArgumentOutOfRangeException()
        };    
    }
}