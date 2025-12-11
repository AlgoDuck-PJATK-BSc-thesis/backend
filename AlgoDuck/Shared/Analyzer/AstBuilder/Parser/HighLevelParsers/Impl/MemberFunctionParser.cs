using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;

public class MemberFunctionParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    MidLevelParser(tokens, filePosition, symbolTableBuilder),
    IMemberFunctionParser
{
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    public AstNodeMemberFunc<T> ParseMemberFunctionDeclaration<T>(AstNodeTypeMember<T> typeMember) where T: IType<T>
    {
        var memberFunc = new AstNodeMemberFunc<T>();
        memberFunc.SetMemberType(typeMember.GetMemberType()!);
        
        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier is not null)
        {
            memberFunc.AccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        memberFunc.Modifiers = ParseModifiers([MemberModifier.Static, MemberModifier.Final, MemberModifier.Abstract, MemberModifier.Strictfp, MemberModifier.Default]);

        ParseGenericDeclaration(memberFunc);
        
        ParseMemberFuncReturnType(memberFunc);
        
        if (!memberFunc.IsConstructor)
        {
            memberFunc.Identifier = ConsumeIfOfType("identifier", TokenType.Ident);
            _symbolTableBuilder.DefineSymbol(new MethodSymbol
            {
                Name = memberFunc.Identifier!.Value!, 
            });  
        }

        ParseMemberFunctionArguments(memberFunc);
        
        ParseThrowsDirective(memberFunc);

        // SkipIfOfType(TokenType.Semi);
        if (SkipIfOfType(TokenType.Semi)) return memberFunc;

        memberFunc.FuncScope = ParseStatementScope();
        return memberFunc;
    }

    public void ParseMemberFuncReturnType<T>(AstNodeMemberFunc<T> memberFunc) where T: IType<T>
    {
        if (CheckTokenType(TokenType.Ident) && PeekToken()!.Value! ==
            memberFunc.GetMemberType()!.GetIdentifier()!.Value)
        {
            memberFunc.IsConstructor = true;
            memberFunc.FuncReturnType = new ComplexTypeDeclaration
            {
                Identifier = ConsumeToken().Value!
            };
            return;
        }

        memberFunc.FuncReturnType = ParseType();
    }

    public void ParseMemberFunctionArguments<T>(AstNodeMemberFunc<T> memberFunc) where T: IType<T>
    {
        ConsumeIfOfType("'('", TokenType.OpenParen);
        _symbolTableBuilder.EnterScope();
        List<AstNodeScopeMemberVar> funcArguments = [];

        while (!CheckTokenType(TokenType.CloseParen))
        {
            var functionArgument = new AstNodeScopeMemberVar
            {
                VarModifiers = ParseModifiers([MemberModifier.Final]),
                Type = ParseStandardType(),
                Identifier = ConsumeIfOfType("identifier", TokenType.Ident)
            };
                
            funcArguments.Add(functionArgument);
            _symbolTableBuilder.DefineSymbol(new VariableSymbol
            {
                Name = functionArgument.Identifier.Value!,
                SymbolType = functionArgument.Type,
            });

            if (CheckTokenType(TokenType.Comma))
            {
                ConsumeToken();
            }
            else if (!CheckTokenType(TokenType.CloseParen))
            {
                throw new JavaSyntaxException("expected ')'");
            }
        }
        memberFunc.FuncArgs = funcArguments;
        _symbolTableBuilder.ExitScope();
        ConsumeIfOfType(")", TokenType.CloseParen);
    }

    private void ParseThrowsDirective<T>(AstNodeMemberFunc<T> memberFunc) where T: IType<T>
    {
        if (!CheckTokenType(TokenType.Throws)) return;
        ConsumeToken(); // consume throws directive
        while (CheckTokenType(TokenType.Comma, 1))
        {
            memberFunc.ThrownExceptions.Add(ConsumeIfOfType("exception identifier", TokenType.Ident));
            ConsumeToken(); // consume ,
        }
        memberFunc.ThrownExceptions.Add(ConsumeIfOfType("exception identifier", TokenType.Ident));
        
    }
}