using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.HighLevelParsers.Impl;
using ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class ClassMemberParser(List<Token> tokens, FilePosition filePosition) : HighLevelParser(tokens, filePosition), IClassMemberParser
{
    private readonly FilePosition _filePosition = filePosition;
    private readonly List<Token> _tokens = tokens;

    public AstNodeClassMember ParseClassMember(AstNodeCLassScope classScope)
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
        
        AstNodeClassMember classMember = new()
        {
            OwnerClassScope = classScope
        };
        if (CheckTokenType(TokenType.Assign, forwardOffset+1) || CheckTokenType(TokenType.Semi, forwardOffset+1)) //variable declaration
        {
            classMember.ClassMember = new MemberVariableParser(_tokens, _filePosition).ParseMemberVariableDeclaration(classMember);
        }
        else if (CheckTokenType(TokenType.OpenParen, forwardOffset+1)) //function declaration
        {
            classMember.ClassMember = new MemberFunctionParser(_tokens, _filePosition).ParseMemberFunctionDeclaration(classMember);
        }
        else if (CheckTokenType(TokenType.OpenCurly, forwardOffset+1))
        {
            classMember.ClassMember = new ClassParser(_tokens, _filePosition).ParseClass([MemberModifier.Final, MemberModifier.Static]);
        }
        
        return classMember;
    }
}