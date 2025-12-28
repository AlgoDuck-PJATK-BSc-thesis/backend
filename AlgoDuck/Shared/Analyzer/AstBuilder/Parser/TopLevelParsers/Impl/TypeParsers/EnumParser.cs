using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.HighLevelParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Impl;

public class EnumParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    HighLevelParser(tokens, filePosition, symbolTableBuilder),
    IEnumParser
{
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;
    public AstNodeEnum ParseEnum()
    {
        /*
         * Turns out the java implementation of enum is significantly more complex than I had initially anticipated
         * Now while I would love to dig into the details, since it seems like a very good feature of the language.
         * The limited time paired with my use case together make a comprehensive enum parser feel a bit scope-creepy
         * Hence what you see below. As long as the parser does not crash when encountering a complex enum. It's doing its job correctly.
         */
        var enumAstNode = new AstNodeEnum();
        
        var accessModifier = TokenIsAccessModifier(PeekToken());
        if (accessModifier != null)
        {
            enumAstNode.AccessModifier = accessModifier.Value;
            ConsumeToken();
        }

        while (PeekToken()?.Type == TokenType.At)
        {
            enumAstNode.AddAnnotation(ParseAnnotation());
        }
        
        enumAstNode.Modifiers = ParseModifiers([MemberModifier.Static]);
        
        ConsumeIfOfType("enum", TokenType.Enum);

        enumAstNode.Name = ConsumeIfOfType("enum name", TokenType.Ident);
        
        _symbolTableBuilder.DefineSymbol(new TypeSymbol<AstNodeEnum>
        {
            AssociatedType = enumAstNode,
            Name = enumAstNode.Name.Value!,
        });
        
        enumAstNode.TypeScope = ConsumeScope();
        return enumAstNode;
    }

    private AstNodeTypeScope<AstNodeEnum> ConsumeScope()
    {
        _symbolTableBuilder.EnterScope();
        
        var startPos = _filePosition.GetFilePos();
    
        ConsumeIfOfType("{", TokenType.OpenCurly);
    
        var depth = 1;
        while (depth > 0 && PeekToken() != null)
        {
            var tokenType = PeekToken()?.Type;
            
            if (tokenType == TokenType.OpenCurly) depth++;
            if (tokenType == TokenType.CloseCurly) depth--;
            ConsumeToken();
        }
    
        var scope = new AstNodeTypeScope<AstNodeEnum>
        {
            OwnScope = _symbolTableBuilder.CurrentScope,
            ScopeBeginOffset = startPos,
            ScopeEndOffset = _filePosition.GetFilePos()
        };
        _symbolTableBuilder.ExitScope();
        return scope;
    }
}