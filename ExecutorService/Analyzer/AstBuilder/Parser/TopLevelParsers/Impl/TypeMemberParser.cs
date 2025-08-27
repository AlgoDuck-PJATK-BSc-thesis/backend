using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class TypeMemberParser(List<Token> tokens, FilePosition filePosition) : HighLevelParser(tokens, filePosition), ITypeMemberParser
{
    private readonly FilePosition _filePosition = filePosition;
    private readonly List<Token> _tokens = tokens;

    public AstNodeTypeMember<T> ParseTypeMember<T>(T member) where T: IType<T>
    {
        var forwardOffset = 0;
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

        var typeMember = new AstNodeTypeMember<T>();
        
        typeMember.SetMemberType(member);
        
        if (CheckTokenType(TokenType.Assign, forwardOffset+1) || CheckTokenType(TokenType.Semi, forwardOffset+1)) //variable declaration
        {
            typeMember.ClassMember = new MemberVariableParser(_tokens, _filePosition).ParseMemberVariableDeclaration(typeMember);
        }
        else if (CheckTokenType(TokenType.OpenParen, forwardOffset+1)) //function declaration
        {
            typeMember.ClassMember = new MemberFunctionParser(_tokens, _filePosition).ParseMemberFunctionDeclaration(typeMember);
        }
        else if (CheckTokenType(TokenType.OpenCurly, forwardOffset+1))
        {
            var astNodeMemberClass = new AstNodeMemberClass<T>
            {
                Class = new ClassParser(_tokens, _filePosition).ParseClass([MemberModifier.Final, MemberModifier.Static, MemberModifier.Abstract])
            };
            astNodeMemberClass.SetMemberType(member);
            typeMember.ClassMember = astNodeMemberClass;
        }
        
        return typeMember;
    }
}