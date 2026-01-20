using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class TypeParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder)
    : ParserCore(tokens, filePosition, symbolTableBuilder), ITypeParser
{
    private const int MaxArrayDimensions = 255;
    private const int MaxGenericParams = 100;
    private const int MaxQualifiedNameParts = 50;

    private static readonly HashSet<TokenType> SimpleTypes =
    [
        TokenType.Byte, TokenType.Short, TokenType.Int, TokenType.Long,
        TokenType.Float, TokenType.Double, TokenType.Char,
        TokenType.Boolean, TokenType.String
    ];

    public OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration> ParseType()
    {
        if (PeekToken() == null)
        {
            throw new JavaSyntaxException("Unexpected end of input while parsing type");
        }
        
        if (PeekToken()!.Type == TokenType.Void)
        {
            ConsumeToken();
            return SpecialMemberType.Void;
        }

        var isArrayBracket = PeekToken(1)?.Type == TokenType.OpenBrace;
        var isVarArgs = PeekToken(1)?.Type == TokenType.Dot
                        && PeekToken(2)?.Type == TokenType.Dot
                        && PeekToken(3)?.Type == TokenType.Dot;

        if (isArrayBracket || isVarArgs)
        {
            return ParseArrayType();
        }

        if (TokenIsSimpleType(PeekToken()))
        {
            return ParseSimpleType(ConsumeToken());
        }

        if (PeekToken()!.Type == TokenType.Ident)
        {
            return ParseComplexTypDeclaration();
        }

        throw new JavaSyntaxException($"Unexpected token: {PeekToken()?.Type} {PeekToken()?.FilePos}");
    }

    public OneOf<MemberType, ArrayType, ComplexTypeDeclaration> ParseStandardType()
    {
        return ParseType().Match<OneOf<MemberType, ArrayType, ComplexTypeDeclaration>>(
            t1 => t1,
            t2 => throw new JavaSyntaxException("type void not valid at this point"),
            t3 => t3,
            t4 => t4);
    }

    private ArrayType ParseArrayType()
    {
        var arrayType = new ArrayType();
        
        if (PeekToken() == null)
        {
            throw new JavaSyntaxException("Unexpected end of input while parsing array type");
        }
        
        if (TokenIsSimpleType(PeekToken()))
        {
            arrayType.BaseType = ParseSimpleType(ConsumeToken());
        }
        else if (PeekToken()!.Type == TokenType.Ident)
        {
            arrayType.BaseType = ParseComplexTypDeclaration();
        }
        else
        {
            throw new JavaSyntaxException($"Expected type, got: {PeekToken()!.Type}");
        }

        if (CheckTokenType(TokenType.Dot))
        {
            ConsumeIfOfType(".", TokenType.Dot);
            ConsumeIfOfType(".", TokenType.Dot);
            ConsumeIfOfType(".", TokenType.Dot);
            arrayType.Dim = 1;
            arrayType.IsVarArgs = true;
            return arrayType;
        }

        ConsumeIfOfType("[", TokenType.OpenBrace);
        ConsumeIfOfType("]", TokenType.CloseBrace);
        var dim = 1;
        
        while (CheckTokenType(TokenType.OpenBrace) && CheckTokenType(TokenType.CloseBrace, 1))
        {
            if (++dim > MaxArrayDimensions)
            {
                throw new JavaSyntaxException($"Array dimension exceeds maximum of {MaxArrayDimensions}");
            }
            TryConsumeNTokens(2);
        }

        arrayType.Dim = dim;
        return arrayType;
    }

    public bool TokenIsSimpleType(Token? token)
    {
        if (token is null)
        {
            return false; 
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
            _ => throw new ArgumentOutOfRangeException(nameof(token), $"Not a simple type: {token.Type}")
        };
    }

    public ComplexTypeDeclaration ParseComplexTypDeclaration()
    {
        return WithRecursionLimit(() =>
        {
            symbolTableBuilder.IncrementParseOps();

            var complexTypeDeclaration = new ComplexTypeDeclaration
            {
                Identifier = ConsumeIfOfType("Type name", TokenType.Ident).Value!
            };

            var nameParts = 1;
            while (CheckTokenType(TokenType.Dot))
            {
                if (++nameParts > MaxQualifiedNameParts)
                {
                    throw new JavaSyntaxException($"Qualified name exceeds maximum of {MaxQualifiedNameParts} parts");
                }
                ConsumeToken();
                var nextPart = ConsumeIfOfType("Type name", TokenType.Ident).Value!;
                complexTypeDeclaration.Identifier = $"{complexTypeDeclaration.Identifier}.{nextPart}";
            }

            if (!CheckTokenType(TokenType.OpenChevron)) return complexTypeDeclaration;

            ConsumeToken(); // consume 
            complexTypeDeclaration.GenericInitializations = [];
            
            var genericCount = 0;
            while (!CheckTokenType(TokenType.CloseChevron) && PeekToken() != null)
            {
                if (++genericCount > MaxGenericParams)
                {
                    throw new JavaSyntaxException($"Generic parameters exceed maximum of {MaxGenericParams}");
                }
                
                complexTypeDeclaration.GenericInitializations.Add(ParseGenericInitialization());
                
                if (CheckTokenType(TokenType.Comma))
                {
                    ConsumeToken();
                }
                else if (!CheckTokenType(TokenType.CloseChevron))
                {
                    throw new JavaSyntaxException($"Expected ',' or '>' in generic parameters, got: {PeekToken()?.Type}");
                }
            }

            ConsumeIfOfType(">", TokenType.CloseChevron);

            return complexTypeDeclaration;
        });
    }

    private GenericInitialization ParseGenericInitialization()
    {
        return WithRecursionLimit(() =>
        {
            symbolTableBuilder.IncrementParseOps();

            var initialization = new GenericInitialization();
            
            if (PeekToken() == null)
            {
                throw new JavaSyntaxException("Unexpected end of input while parsing generic initialization");
            }
            
            if (CheckTokenType(TokenType.Wildcard))
            {
                ConsumeToken(); // consume ?
                initialization.IsWildCard = true;
                
                if (CheckTokenType(TokenType.Extends))
                {
                    ConsumeToken();
                    initialization.ExtendsTypes = [];
                    
                    var boundsCount = 0;
                    do
                    {
                        if (++boundsCount > MaxGenericParams)
                        {
                            throw new JavaSyntaxException($"Type bounds exceed maximum of {MaxGenericParams}");
                        }
                        
                        if (boundsCount > 1) ConsumeToken(); // consume &
                        initialization.ExtendsTypes.Add(ParseComplexTypDeclaration());
                        
                    } while (CheckTokenType(TokenType.BitAnd));
                }
                else if (CheckTokenType(TokenType.Super))
                {
                    ConsumeToken();
                    initialization.SupersType = ParseComplexTypDeclaration();
                }
            }
            else
            {
                initialization.Identifier = ParseComplexTypDeclaration();
            }

            return initialization;
        });
    }
}