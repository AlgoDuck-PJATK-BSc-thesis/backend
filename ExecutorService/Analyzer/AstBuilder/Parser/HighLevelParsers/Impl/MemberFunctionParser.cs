using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers;
using ExecutorService.Errors.Exceptions;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;

public class MemberFunctionParser(List<Token> tokens, FilePosition filePosition) :
    MidLevelParser(tokens, filePosition),
    IMemberFunctionParser
{
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
        }

        ParseMemberFunctionArguments(memberFunc);
        
        ParseThrowsDirective(memberFunc);

        if (CheckTokenType(TokenType.Semi))
        {
            ConsumeToken();
        }
        else
        {
            memberFunc.FuncScope = ParseStatementScope();
        }
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
        List<AstNodeScopeMemberVar> funcArguments = [];

        while (!CheckTokenType(TokenType.CloseParen))
        {
            if (CheckTokenType(TokenType.Ident) || CheckTokenType(TokenType.Ident, 1)) // this is not great
            {
                var functionArgument = new AstNodeScopeMemberVar();
                if (CheckTokenType(TokenType.Final))
                {
                    functionArgument.VarModifiers = new List<MemberModifier>([MemberModifier.Final]);
                    ConsumeToken();
                }

                ParseType().Switch(
                    t1 => functionArgument.Type = t1,
                    t2 => throw new JavaSyntaxException("can't declare variable of type void"),
                    t3 => functionArgument.Type = t3,
                    t4 => functionArgument.Type = t4
                );
                
                functionArgument.Identifier = ConsumeIfOfType("identifier", TokenType.Ident);
                funcArguments.Add(functionArgument);
            }
            else
            {
                funcArguments.Add(ParseScopeMemberVariableDeclaration([MemberModifier.Final]));
            }
            
            if (CheckTokenType(TokenType.Comma))
            {
                ConsumeToken();
            }
            else if (!CheckTokenType(TokenType.CloseParen))
            {
                throw new JavaSyntaxException("unexpected token");
            }
        }
        memberFunc.FuncArgs = funcArguments;
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