using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;

namespace ExecutorService.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;

public class ScopeVariableParser(List<Token> tokens, FilePosition filePosition) : LowLevelParser(tokens, filePosition), IScopeVariableParser
{
    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers)
    {
        AstNodeScopeMemberVar scopedVar = new();

        var modifiers = ParseModifiers([MemberModifier.Static, MemberModifier.Final]);

        if (modifiers.Any(modifier => !permittedModifiers.Contains(modifier))) throw new JavaSyntaxException("Illegal modifier");
        
        var varType = ParseType();
        
        if (varType == null)
        {
            throw new JavaSyntaxException("Type required");
        }
        
        scopedVar.Type = varType switch
        {
            { IsT0: true } => varType.Value.AsT0,
            { IsT1: true } => throw new JavaSyntaxException("cannot declare variable of type void"), 
            { IsT2: true } => varType.Value.AsT2,
            { IsT3: true } => varType.Value.AsT3,
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
}