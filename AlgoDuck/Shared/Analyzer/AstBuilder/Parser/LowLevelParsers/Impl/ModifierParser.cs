using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class ModifierParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    ParserCore(tokens, filePosition, symbolTableBuilder),
    IModifierParser
{
    public AccessModifier? TokenIsAccessModifier(Token? token)
    {
        return token?.Type switch
        {
            TokenType.Private => AccessModifier.Private,
            TokenType.Protected => AccessModifier.Protected,
            TokenType.Public => AccessModifier.Public,
            _ => null
        };
    }
    public bool TokenIsModifier(Token token)
    {
        return token.Type switch
        {
            TokenType.Static or TokenType.Final or TokenType.Abstract or TokenType.Default or TokenType.Strictfp => true,
            _ => false
        };
    }
    
    public List<MemberModifier> ParseModifiers(List<MemberModifier> legalModifiers)
    {
        List<MemberModifier> modifiers = [];
        
        /*
         * If token is Type it is a non-generic method
         * If token is Open chevron it is either generic class or method
         * If token is ident it is a non-generic class
         */
        while (PeekToken() != null && TokenIsModifier(PeekToken()!) )
        {
            var consumed = ConsumeToken();
            var modifier = consumed.Type switch
            {
                TokenType.Static => MemberModifier.Static,
                TokenType.Final => MemberModifier.Final,
                TokenType.Abstract => MemberModifier.Abstract,
                TokenType.Default => MemberModifier.Default,
                TokenType.Strictfp => MemberModifier.Strictfp,
                _ => throw new JavaSyntaxException("unexpected token")
            };
            if (legalModifiers.Contains(modifier))
            {
                modifiers.Add(modifier);
            }
            else
            {
                throw new JavaSyntaxException($"illegal modifier: {modifier}");
            }
        }
        
        return modifiers;
    }
}