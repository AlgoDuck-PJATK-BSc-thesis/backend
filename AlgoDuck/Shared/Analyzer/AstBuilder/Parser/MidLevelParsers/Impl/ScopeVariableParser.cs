using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;

public class ScopeVariableParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : LowLevelParser(tokens, filePosition, symbolTableBuilder), IScopeVariableParser
{
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;
    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers)
    {
        
        AstNodeScopeMemberVar scopedVar = new()
        {
            VarModifiers = ParseModifiers([MemberModifier.Static, MemberModifier.Final]),
            Type = ParseStandardType(),
            Identifier = ConsumeIfOfType("ident", TokenType.Ident)
        };

        _symbolTableBuilder.DefineSymbol(new VariableSymbol
        {
            Name = scopedVar.Identifier!.Value!,
            SymbolType = scopedVar.Type,
        });
        
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