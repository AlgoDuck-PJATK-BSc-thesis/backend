using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Abstr;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers;

namespace ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;

public class MemberFunctionParser(List<Token> tokens, FilePosition filePosition) :
    MidLevelParser(tokens, filePosition),
    IMemberFunctionParser
{
    public AstNodeClassMemberFunc ParseMemberFunctionDeclaration(AstNodeClassMember classMember)
    {
        AstNodeClassMemberFunc memberFunc = new()
        {
            OwnerClassMember = classMember
        };
        
        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier is not null)
        {
            memberFunc.AccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        memberFunc.Modifiers = ParseModifiers([MemberModifier.Static, MemberModifier.Final]);

        ParseGenericDeclaration(memberFunc);
        
        ParseMemberFuncReturnType(memberFunc);
        
        if (!memberFunc.IsConstructor)
        {
            memberFunc.Identifier = ConsumeIfOfType(TokenType.Ident, "identifier");
        }

        ParseMemberFunctionArguments(memberFunc);
        
        ParseThrowsDirective(memberFunc);
        
        memberFunc.FuncScope = ParseStatementScope();
        return memberFunc;
    }

    public void ParseMemberFuncReturnType(AstNodeClassMemberFunc memberFunc)
    {
        if (CheckTokenType(TokenType.Ident) && PeekToken()!.Value! ==
            memberFunc.OwnerClassMember!.OwnerClassScope!.OwnerClass!.Identifier!.Value)
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

    public void ParseMemberFunctionArguments(AstNodeClassMemberFunc memberFunc)
    {
        ConsumeIfOfType(TokenType.OpenParen, "'('");
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
                
                functionArgument.Identifier = ConsumeIfOfType(TokenType.Ident, "identifier");
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
        ConsumeIfOfType(TokenType.CloseParen, ")");
    }

    private void ParseThrowsDirective(AstNodeClassMemberFunc memberFunc)
    {
        if (!CheckTokenType(TokenType.Throws)) return;
        ConsumeToken(); // consume throws directive
        while (CheckTokenType(TokenType.Comma, 1))
        {
            memberFunc.ThrownExceptions.Add(ConsumeIfOfType(TokenType.Ident, "exception identifier"));
            ConsumeToken(); // consume ,
        }
        memberFunc.ThrownExceptions.Add(ConsumeIfOfType(TokenType.Ident, "exception identifier"));
        
    }
}