using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;
using ExecutorService.Errors.Exceptions;

namespace ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;

public class ScopeVariableParser(List<Token> tokens, FilePosition filePosition) : LowLevelParser(tokens, filePosition), IScopeVariableParser
{
    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers)
    {
        AstNodeScopeMemberVar scopedVar = new()
        {
            VarModifiers = ParseModifiers([MemberModifier.Static, MemberModifier.Final])
        };


        if (scopedVar.VarModifiers.Any(modifier => !permittedModifiers.Contains(modifier))) throw new JavaSyntaxException("Illegal modifier");

        var varType = ParseType();
        
        scopedVar.Type = varType switch
        {
            { IsT0: true } => varType.AsT0,
            { IsT1: true } => throw new JavaSyntaxException("cannot declare variable of type void"), 
            { IsT2: true } => varType.AsT2,
            { IsT3: true } => varType.AsT3,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        scopedVar.Identifier = ConsumeIfOfType(TokenType.Ident, "ident");
        if (CheckTokenType(TokenType.Assign))//TODO suboptimal
        {
            ConsumeToken();
            while (!CheckTokenType(TokenType.Semi))
            {
                scopedVar.VariableValue = ParseExpr();
            }
        }

        if (CheckTokenType(TokenType.Semi))
        {
            ConsumeToken(); 
        }
        return scopedVar;
    }
}