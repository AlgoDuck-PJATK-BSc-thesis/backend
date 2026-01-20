using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;


namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class TypeMemberParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : HighLevelParser(tokens, filePosition, symbolTableBuilder), ITypeMemberParser
{
    private readonly FilePosition _filePosition = filePosition;
    private readonly List<Token> _tokens = tokens;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;

    public AstNodeTypeMember<T> ParseTypeMember<T>(T member) where T: BaseType<T>
    {
        return WithRecursionLimit(() =>
        {
            _symbolTableBuilder.IncrementParseOps();
            if (CheckTokenType(TokenType.OpenBrace))
            {
                SkipBraceBlock();
                throw new JavaSyntaxException("Not actually illegal syntax but we can't return anything here");
            }

            if (CheckTokenType(TokenType.Static) && CheckTokenType(TokenType.OpenBrace, 1))
            {
                
                ConsumeToken();
                SkipBraceBlock();
                throw new JavaSyntaxException("Not actually illegal syntax but we can't return anything here");
            }

            var forwardOffset = 0;
            const int maxLookahead = 10000;
            /*
             * workaround to generic idents being caught as function names. Probably a better way to do it
             * looks forward until an identifier token is found, when that happens it verifies whether the succeeding token is a
             * - open parentheses - "(" - we parse for a method
             * - assignment or semicolon - "=" or ";" we parse for variable declaration
             * - open curly brace - "{" we parse for inline class declaration
             **/
            while
                (PeekToken(forwardOffset) != null && !((CheckTokenType(TokenType.Ident, forwardOffset) && CheckTokenType(TokenType.OpenParen, forwardOffset + 1)) ||
                                                       CheckTokenType(TokenType.Semi, forwardOffset) || 
                                                       CheckTokenType(TokenType.Class, forwardOffset) ||
                                                       CheckTokenType(TokenType.Interface, forwardOffset) ||
                                                       CheckTokenType(TokenType.Record, forwardOffset) ||
                                                       CheckTokenType(TokenType.Enum, forwardOffset) ||
                                                       (CheckTokenType(TokenType.At, forwardOffset) && CheckTokenType(TokenType.Ident, forwardOffset + 1)) ||
                                                       CheckTokenType(TokenType.Semi, forwardOffset) ||
                                                       CheckTokenType(TokenType.Assign, forwardOffset) || IsInitBlock(forwardOffset)))
            {
                forwardOffset++;
                if (forwardOffset >= maxLookahead)
                {
                    throw new JavaParseComplexityExceededException("Member declaration too complex");
                }
            }



            if (IsInitBlock(forwardOffset))
            {
                for (var i = 0; i < forwardOffset; i++)
                {
                    ConsumeToken();
                }
            
                SkipBraceBlock();
                throw new JavaSyntaxException("Not actually illegal syntax but we can't return anything here");
            }

            var typeMember = new AstNodeTypeMember<T>();

            typeMember.SetMemberType(member);


            if (CheckTokenType(TokenType.Assign, forwardOffset) ||
                CheckTokenType(TokenType.Semi, forwardOffset)) 
            {
                    new MemberVariableParser(_tokens, _filePosition, _symbolTableBuilder)
                        .ParseMemberVariableDeclaration(typeMember);
            }
            else if (CheckTokenType(TokenType.Ident, forwardOffset) &&
                     CheckTokenType(TokenType.OpenParen, forwardOffset + 1)) 
            {
                typeMember.ClassMember =
                    new MemberFunctionParser(_tokens, _filePosition, _symbolTableBuilder)
                        .ParseMemberFunctionDeclaration(typeMember);
            }
            else if (CheckTokenType(TokenType.Class, forwardOffset))
            {
                var astNodeMemberClass = new AstNodeMemberClass<T>
                {
                    Class = new ClassParser(_tokens, _filePosition, _symbolTableBuilder).ParseClass([
                        MemberModifier.Final, MemberModifier.Static, MemberModifier.Abstract
                    ])
                };
                astNodeMemberClass.SetMemberType(member);
                typeMember.ClassMember = astNodeMemberClass;
            }
            else if (CheckTokenType(TokenType.Interface, forwardOffset))
            {
                new InterfaceParser(_tokens, _filePosition, _symbolTableBuilder).ParseInterface([
                    MemberModifier.Final, MemberModifier.Static, MemberModifier.Abstract
                ]);
            }else if (CheckTokenType(TokenType.Record, forwardOffset) || CheckTokenType(TokenType.Enum, forwardOffset) ||
                      (CheckTokenType(TokenType.At, forwardOffset) && CheckTokenType(TokenType.Ident, forwardOffset)))
            {
                for (var i = 0; i < forwardOffset; i++)
                {
                    ConsumeToken();
                }
                ConsumeToken(); // consume 'record' or 'enum' or '@ident'
    
                while (!CheckTokenType(TokenType.OpenBrace) && PeekToken() != null)
                {
                    ConsumeToken();
                }
                SkipBraceBlock();
                throw new JavaSyntaxException("Records and enums are not supported");
            }
            else
            {
                throw new JavaSyntaxException("Could not resolve type member");
            }

            return typeMember;
        });
    }
    
    private bool IsInitBlock(int offset = 0)
    {
        if (CheckTokenType(TokenType.OpenCurly, offset)) 
        {
            return true;
        }
    
        if (CheckTokenType(TokenType.Static, offset) && CheckTokenType(TokenType.OpenCurly, offset + 1)) 
        {
            return true;
        }
    
        return false;
    }
    
    private void SkipBraceBlock()
    {
        if (!CheckTokenType(TokenType.OpenCurly)) 
        {
            return;
        }
    
        ConsumeToken();
    
        var braceDepth = 1;
        while (braceDepth > 0 && PeekToken() != null)
        {
            if (CheckTokenType(TokenType.OpenCurly)) 
            {
                braceDepth++;
            }
            else if (CheckTokenType(TokenType.CloseCurly)) 
            {
                braceDepth--;
            }
            ConsumeToken();
        }
        TryConsumeTokenOfType(TokenType.CloseCurly, out var _); 
    }
    
}