using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class ModifierParser(List<Token> tokens, FilePosition filePosition) :
    ParserCore(tokens, filePosition),
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
            TokenType.Static or TokenType.Final => true,
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