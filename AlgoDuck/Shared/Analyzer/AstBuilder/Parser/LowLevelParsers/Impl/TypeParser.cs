using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class TypeParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : ParserCore(tokens, filePosition, symbolTableBuilder), ITypeParser
{
    
    private static readonly HashSet<TokenType> SimpleTypes =
    [
        TokenType.Byte, TokenType.Short, TokenType.Int, TokenType.Long,
        TokenType.Float, TokenType.Double, TokenType.Char,
        TokenType.Boolean, TokenType.String
    ];
    
    
    // TODO clean up the null checks here
    public OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration> ParseType()
    {
        if (PeekToken() != null && PeekToken()!.Type == TokenType.Void)
        {
            ConsumeToken();
            return SpecialMemberType.Void;
        }


        if (PeekToken() != null && (PeekToken(1)!.Type == TokenType.Dot || PeekToken(1)!.Type == TokenType.OpenBrace))
        {
            return ParseArrayType();
        }
        
        if (TokenIsSimpleType(PeekToken()))
        {
            return ParseSimpleType(ConsumeToken());
        }

        if (PeekToken() != null && PeekToken()!.Type == TokenType.Ident) return ParseComplexTypDeclaration();
        
        throw new JavaSyntaxException("huhhhhhhh");
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
        if (TokenIsSimpleType(PeekToken()))
        {
            arrayType.BaseType = ParseSimpleType(ConsumeToken());
        }else if (PeekToken()!.Type == TokenType.Ident)
        {
            arrayType.BaseType = ParseComplexTypDeclaration();
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
            TryConsumeNTokens(2);
            dim++;
        }

        arrayType.Dim = dim;
        return arrayType;
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

    public ComplexTypeDeclaration ParseComplexTypDeclaration()
    {
        var complexTypeDeclaration = new ComplexTypeDeclaration
        {
            Identifier = ConsumeIfOfType("Type name", TokenType.Ident).Value!
        };
        if (!CheckTokenType(TokenType.OpenChevron)) return complexTypeDeclaration;

        ConsumeToken(); //consume <
        complexTypeDeclaration.GenericInitializations = [];
        while (CheckTokenType(TokenType.Comma, 1))
        {
            complexTypeDeclaration.GenericInitializations.Add(ParseGenericInitialization());
            ConsumeIfOfType(",", TokenType.Comma);
        }
        complexTypeDeclaration.GenericInitializations.Add(ParseGenericInitialization());
        
        ConsumeIfOfType(">", TokenType.CloseChevron);

        return complexTypeDeclaration;
    }

    private GenericInitialization ParseGenericInitialization()
    {
        var initialization = new GenericInitialization();
        if (CheckTokenType(TokenType.Wildcard))
        {
            ConsumeToken(); // consume ?
            initialization.IsWildCard = true;
            if (CheckTokenType(TokenType.Extends))
            {
                ConsumeToken();
                initialization.ExtendsTypes = [];
                while (CheckTokenType(TokenType.BitAnd, 1))
                {
                    initialization.ExtendsTypes.Add(ParseComplexTypDeclaration());
                    ConsumeToken(); // consume &
                }
                initialization.ExtendsTypes.Add(ParseComplexTypDeclaration());
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
    }
}