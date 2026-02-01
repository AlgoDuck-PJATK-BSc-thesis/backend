using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.MidLevelParsers.Impl;

public class ScopeVariableParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) : LowLevelParser(tokens, filePosition, symbolTableBuilder), IScopeVariableParser
{
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;
    public AstNodeScopeMemberVar ParseScopeMemberVariableDeclaration(MemberModifier[] permittedModifiers)
    {
        var modifiers = ParseModifiers([MemberModifier.Static, MemberModifier.Final]);
        var type = ParseStandardType();
        
        var scopedVar = ParseSingleVariable(modifiers, type);
        
        while (CheckTokenType(TokenType.Comma))
        {
            ConsumeToken();
            ParseSingleVariable(modifiers, type);
        }

        if (CheckTokenType(TokenType.Semi))
        {
            ConsumeToken();
        }
        
        return scopedVar;
    }

    private AstNodeScopeMemberVar ParseSingleVariable(
        List<MemberModifier> modifiers,
        OneOf<MemberType, ArrayType, ComplexTypeDeclaration> type)
    {
        var scopedVar = new AstNodeScopeMemberVar
        {
            VarModifiers = modifiers,
            Type = type,
            Identifier = ConsumeIfOfType("ident", TokenType.Ident)
        };

        _symbolTableBuilder.DefineSymbol(new VariableSymbol
        {
            ScopeMemberVar = scopedVar,
            Name = scopedVar.Identifier!.Value!,
            SymbolType = scopedVar.Type,
        });

        if (CheckTokenType(TokenType.Assign))
        {
            ConsumeToken();
            scopedVar.VariableValue = ParseExpr();
        }

        return scopedVar;
    }
}