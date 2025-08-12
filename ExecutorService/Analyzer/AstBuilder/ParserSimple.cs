using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder;

public interface IParser
{
    public AstNodeProgram ParseProgram(List<List<Token>> compilationUnits);
}

public class ParserSimple : IParser
{
    private int _currPos;
    private List<List<Token>> _compilationUnits = [];
    private List<Token> _tokens = [];
    
    private static readonly HashSet<TokenType> SimpleTypes =
    [
        TokenType.Byte, TokenType.Short, TokenType.Int, TokenType.Long,
        TokenType.Float, TokenType.Double, TokenType.Char,
        TokenType.Boolean, TokenType.String
    ];

    public AstNodeProgram ParseProgram(List<List<Token>> compilationUnits)
    {
        _compilationUnits = compilationUnits;
        AstNodeProgram program = new();
        foreach (var compilationUnit in compilationUnits)
        {
            _currPos = 0;
            _tokens = compilationUnit;
            while (PeekToken() is not null)
            {
                program.ProgramCompilationUnits.Add(ParseCompilationUnit());
            }    
        }

        return program;
    }

    private AstNodeCompilationUnit ParseCompilationUnit()
    {
        
        var compilationUnit = new AstNodeCompilationUnit();
        
        if (CheckTokenType(TokenType.Package))
        {
            ConsumeIfOfType(TokenType.Package, "not gonna happen, put here for readability");
            AstNodePackage package = new();
            ParseImportsAndPackages(package);
            compilationUnit.Package = package;
        }
    
        while (CheckTokenType(TokenType.Import))
        {
            ConsumeIfOfType(TokenType.Import, "not gonna happen, put here for readability");
            AstNodeImport import = new();
            ParseImportsAndPackages(import);
            compilationUnit.Imports.Add(import);
        }
        
        while (PeekToken() != null)
        {
            compilationUnit.CompilationUnitTopLevelStatements.Add(ParseTopLevelStatement(compilationUnit));
        }

        return compilationUnit;
    }

    private OneOf<AstNodeClass> ParseTopLevelStatement(AstNodeCompilationUnit compilationUnit)
    {
        var lookahead = 0;
        while (!(CheckTokenType(TokenType.Class, lookahead) /*|| CheckTokenType(TokenType.Import, lookahead)*/)) 
        {
            lookahead++;
        }

        return PeekToken(lookahead)!.Type switch
        {
            TokenType.Class => ParseClass([MemberModifier.Final]),
            _ => throw new JavaSyntaxException($"Unexpected token {PeekToken(lookahead)!.Type}")
        };
    }

    private AstNodeClass ParseClass(List<MemberModifier> legalModifiers) // perhaps should not focus on grammatical correctness immediately but this is fairly low-hanging fruit
    {
        AstNodeClass nodeClass = new();
        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier != null)
        {
            nodeClass.ClassAccessModifier = accessModifier.Value;
            ConsumeToken();
        }
        nodeClass.ClassModifiers = ParseModifiers(legalModifiers);
        
        ConsumeIfOfType(TokenType.Class, "class");

        nodeClass.Identifier = ConsumeIfOfType(TokenType.Ident, "class name");
        
        ParseGenericDeclaration(nodeClass);
        
        nodeClass.ClassScope = ParseClassScope();
        return nodeClass;
    }

    private void ParseImportsAndPackages(IUriSetter statement) //TODO in Gods name I command thee change this naming
    { 
        var uri = new StringBuilder();
        
        while (PeekToken(1) != null && PeekToken(1)!.Type != TokenType.Semi)
        {
            var uriComponent = ConsumeIfOfType(TokenType.Ident, "uri component").Value!;
            uri.Append($"{uriComponent}.");
            ConsumeToken(); // consume delim
        }

        var lastUriComponentToken = ConsumeIfOfType(TokenType.Ident, "identifier");
        ConsumeIfOfType(TokenType.Semi, "semi colon");
        uri.Append(lastUriComponentToken.Value!);

        statement.SetUri(uri.ToString());
    }

    private AstNodeCLassScope ParseClassScope()
    {

        AstNodeCLassScope classScope = new()
        {
            ScopeBeginOffset = ConsumeIfOfType(TokenType.OpenCurly, "'{'").FilePos
        };
        
        while (!CheckTokenType(TokenType.CloseCurly))
        {
            classScope.ClassMembers.Add(ParseClassMember());
        }
        
        classScope.ScopeEndOffset = ConsumeIfOfType(TokenType.CloseCurly, "'}'").FilePos;
        return classScope;
    }

    private AstNodeClassMemberFunc ParseMemberFunctionDeclaration()
    {
        AstNodeClassMemberFunc memberFunc = new();
        AccessModifier? accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier is not null)
        {
            memberFunc.AccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        memberFunc.Modifiers = ParseModifiers([MemberModifier.Static]);

        ParseGenericDeclaration(memberFunc);

        ParseMemberFuncReturnType(memberFunc);

        memberFunc.Identifier = ConsumeIfOfType(TokenType.Ident, "identifier");

        ParseMemberFunctionArguments(memberFunc);
        
        memberFunc.FuncScope = ParseStatementScope();
        return memberFunc;
    }

    private void ParseMemberFunctionArguments(AstNodeClassMemberFunc memberFunc)
    {
        ConsumeIfOfType(TokenType.OpenParen, "'('");
        List<AstNodeScopeMemberVar> funcArguments = new();

        while (!CheckTokenType(TokenType.CloseParen))
        {
            if (CheckTokenType(TokenType.Ident) || CheckTokenType(TokenType.Ident, 1)) // this is not great
            {
                AstNodeScopeMemberVar genericArgument = new AstNodeScopeMemberVar();
                if (CheckTokenType(TokenType.Final))
                {
                    genericArgument.VarModifiers = new List<MemberModifier>([MemberModifier.Final]);
                    ConsumeToken();
                }
                
                genericArgument.Type = ConsumeToken();
                genericArgument.Identifier = ConsumeIfOfType(TokenType.Ident, "identifier");
                funcArguments.Add(genericArgument);
            }
            else
            {
                funcArguments.Add(ParseScopeMemberVariableDeclaration([MemberModifier.Final]));
            }
            
            if (CheckTokenType(TokenType.Comma))
            {
                ConsumeToken();
            }else if (CheckTokenType(TokenType.CloseParen))
            {
                ConsumeToken();
                memberFunc.FuncArgs = funcArguments;
                return;
            }
            else
            {
                throw new JavaSyntaxException("unexpected token");
            }
        }
    }

    private List<MemberModifier> ParseModifiers(List<MemberModifier> legalModifiers)
    {
        List<MemberModifier> modifiers = new();
        
        /*
         * If token is Type it is a non-generic method
         * If token is Open chevron it is either generic class or method
         * If token is ident it is a non-generic class
         */
        while (PeekToken() != null && TokenIsModifier(PeekToken()!) ) // checking this by type seems suboptimal
        {
            var modifier = ConsumeToken().Type switch
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

    private void ParseGenericDeclaration(IGenericSettable funcOrClass)
    {
        if (!CheckTokenType(TokenType.OpenChevron))
        {
            return;
        }

        ConsumeToken();
        List<Token> genericTypes = [];
        while (!CheckTokenType(TokenType.CloseChevron, 1)) // redundant but I don't wanna do a while(true)
        {
            genericTypes.Add(ConsumeIfOfType(TokenType.Ident, "Type declaration"));
            ConsumeIfOfType(TokenType.Comma, "comma");
        }
        genericTypes.Add(ConsumeIfOfType(TokenType.Ident, "Type declaration"));
        ConsumeIfOfType(TokenType.CloseChevron, "Closing chevron");
        
        funcOrClass.SetGenericTypes(genericTypes);
    }

    private AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers)
    {
        AstNodeScopeMemberVar scopedVar = new();

        var modifiers = ParseModifiers([MemberModifier.Static, MemberModifier.Final]);

        foreach (MemberModifier modifier in modifiers)
        {
            if (!permittedModifiers.Contains(modifier))
            {
                throw new JavaSyntaxException("Illegal modifier");
            }
        }
        
        var parseVar = ParseType();

        
        if (parseVar == null)
        {
            throw new JavaSyntaxException("Type required");
        }
        
        scopedVar.Type = parseVar switch
        {
            { IsT0: true } => parseVar.Value.AsT0,
            { IsT1: true } => throw new JavaSyntaxException("cannot declare variable of type void"), 
            { IsT2: true } => parseVar.Value.AsT2,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        scopedVar.VarModifiers = modifiers;
        scopedVar.Identifier = ConsumeIfOfType(TokenType.Ident, "ident"); 
        if (CheckTokenType(TokenType.Assign))//TODO suboptimal
        {
            ConsumeToken();
            while (!CheckTokenType(TokenType.Semi))
            {
                TryConsume();
            }
        }

        if (CheckTokenType(TokenType.Semi))
        {
            ConsumeToken(); 
        }
        return scopedVar;
    }

    private AstNodeClassMember ParseClassMember()
    {
        int forwardOffset = 0;
        // Console.WriteLine("parsing member");
        /*
         * workaround to generic idents being caught as function names. Probably a better way to do it
         * looks forward until an identifier token is found, when that happens it verifies whether the succeeding token is a
         * - open parentheses - "(" - we parse for a method
         * - assignment or semicolon - "=" or ";" we parse for variable declaration
         * - open curly brace - "{" we parse for inline class declaration
         **/
        while (!(CheckTokenType(TokenType.Ident, forwardOffset) && (CheckTokenType(TokenType.OpenParen, forwardOffset + 1) || CheckTokenType(TokenType.Assign, forwardOffset + 1) || CheckTokenType(TokenType.Semi, forwardOffset + 1) || CheckTokenType(TokenType.OpenCurly, forwardOffset + 1)))) 
        {
            forwardOffset++;
        }
        AstNodeClassMember classMember = new();
        if (CheckTokenType(TokenType.Assign, forwardOffset+1) || CheckTokenType(TokenType.Semi, forwardOffset+1)) //variable declaration
        {
            classMember.ClassMember = ParseMemberVariableDeclaration();
        }
        else if (CheckTokenType(TokenType.OpenParen, forwardOffset+1)) //function declaration
        {
            classMember.ClassMember = ParseMemberFunctionDeclaration();
        }
        else if (CheckTokenType(TokenType.OpenCurly, forwardOffset+1))
        {
            classMember.ClassMember = ParseClass([MemberModifier.Final, MemberModifier.Static]);
        }

        return classMember;
    }

    private AstNodeClassMemberVar ParseMemberVariableDeclaration()
    {
        AstNodeClassMemberVar memberVar = new();
        AccessModifier? accessModifier = TokenIsAccessModifier(PeekToken());
        memberVar.AccessModifier = accessModifier ?? AccessModifier.Public;
        if (accessModifier is not null)
        {
            ConsumeToken();
        }
        memberVar.ScopeMemberVar = ParseScopeMemberVariableDeclaration([MemberModifier.Final, MemberModifier.Static]);
        return memberVar;
    }

    private AstNodeStatement ParseDefaultStat()
    {
        return new AstNodeStatement()
        {
            Variant = new AstNodeStatementUnknown(ConsumeToken())
        };
    }
    
    private AstNodeStatement? ParseStatement()
    {
        switch (PeekToken().Type)
        {
            case TokenType.OpenCurly:
                return ParseScopeWrapper();
            case TokenType.CloseCurly:
                return null;
            default:
                return ParseDefaultStat();
        }
    }

    private AstNodeStatement ParseScopeWrapper()
    {
        return new AstNodeStatement()
        {
            Variant = ParseStatementScope()
        };
    }
    private AstNodeStatementScope ParseStatementScope()
    {

        AstNodeStatementScope scope = new()
        {
            ScopeBeginOffset = ConsumeIfOfType(TokenType.OpenCurly, "'{'").FilePos //consume '{' token
        };

        AstNodeStatement? scopedStatement;
        while (PeekToken() != null && (scopedStatement = ParseStatement()) != null)
        {
            scope.ScopedStatements.Add(scopedStatement);
        }
        
        scope.ScopeEndOffset = ConsumeIfOfType(TokenType.CloseCurly, "'}'").FilePos; //consume '}' token
        
        return scope;
    }

    private Token[] TryConsumeNTokens(int amount = 1)
    {
        Token[] consumedTokens = new Token[amount];
        for (int i = 0; i < amount; i++)
        {
            consumedTokens[i] = TryConsume();
        }

        return consumedTokens;
    }
    private Token TryConsume()
    {
        if (PeekToken() != null)
        {
            return ConsumeToken();
        }

        throw new JavaSyntaxException("token expected");
    }
    private Token ConsumeToken()
    {
        if (_currPos >= _tokens.Count)
        {
            throw new InvalidOperationException("No more tokens");
        }

        return _tokens[_currPos++];
    }
    
    private Token? PeekToken(int offset = 0)
    {
        int accessIndex = _currPos + offset;
        if (accessIndex < _tokens.Count) 
        {
            return _tokens[accessIndex];
        }

        return null;
    }

    private bool CheckTokenType(TokenType tokenType, int offset = 0)
    {
        Token? peekedToken = PeekToken(offset);
        if (peekedToken is not null && peekedToken.Type == tokenType)
        {
            return true;
        }

        return false;
    }
    
    private Token ConsumeIfOfType(TokenType tokenType, string expectedTokenMsg)
    {
        Token? peekedToken = PeekToken();
        if (peekedToken != null && peekedToken.Type == tokenType)
        {
            return ConsumeToken();
        }
        throw new JavaSyntaxException($"Expected {expectedTokenMsg} declaration");
    }
    private AccessModifier? TokenIsAccessModifier(Token? token)
    {
        AccessModifier? result = null;
        if (token is null)
        {
            return result;
        }
        switch (token.Type)
        {
            case TokenType.Private:
                return AccessModifier.Private;
            case TokenType.Protected:
                return AccessModifier.Protected;
            case TokenType.Public:
                return AccessModifier.Public;
            default:
                return null;
        }
    }


    private static bool TokenIsModifier(Token token)
    {
        return token.Type switch
        {
            TokenType.Static or TokenType.Final => true,
            _ => false
        };
    }

    private void ParseMemberFuncReturnType(AstNodeClassMemberFunc memberFunc)
    {
        if (CheckTokenType(TokenType.Ident))
        {
            if (memberFunc.GenericTypes.Find(t => t.Value == PeekToken()!.Value) != null)
            {
                memberFunc.FuncReturnType = ConsumeToken();
            }
        }
        else
        {
            OneOf<MemberType,SpecialMemberType, ArrayType>? type = ParseType();

            if (type == null)
            {
                throw new JavaSyntaxException("return type required");
            }
            
            memberFunc.FuncReturnType = type.Value.Match(
                t0 => t0,
                t1 => t1,
                t2 => (OneOf<MemberType, SpecialMemberType, ArrayType, Token>)t2
            );
        }
    }
    
    private OneOf<MemberType, SpecialMemberType, ArrayType>? ParseType()
    {
        Token token = TryConsume();
        
        if (TokenIsSimpleType(token))
        {
            MemberType type = ParseSimpleType(token);
            int dim = 0;
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
        if (token.Type == TokenType.Void)
        {
            return SpecialMemberType.Void;
        }

        return null;
    }
    
    private bool TokenIsType(Token? token) //TODO name is not really descriptive, not to me at least, change it
    {
        if (token == null)
        {
            throw new JavaSyntaxException("bro what is this");
        }
        
        if (TokenIsSimpleType(token) || token.Type == TokenType.Void)
        {
            return true;
        }
        return false;
    }

    private bool TokenIsSimpleType(Token? token)
    {
        if (token is null)
        {
            throw new JavaSyntaxException("again what are you even giving me?");
        }
        
        return SimpleTypes.Contains(token.Type);
    }
    
    private MemberType ParseSimpleType(Token token)
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