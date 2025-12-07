using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.CoreParsers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class GenericParser(List<Token> tokens, FilePosition filePosition, SymbolTableBuilder symbolTableBuilder) :
    ParserCore(tokens, filePosition, symbolTableBuilder),
    IGenericParser
{
    
    private readonly List<Token> _tokens = tokens;
    private readonly FilePosition _filePosition = filePosition;
    private readonly SymbolTableBuilder _symbolTableBuilder = symbolTableBuilder;
    
    public void ParseGenericDeclaration(IGenericSettable funcOrClass)
    {
        if (!CheckTokenType(TokenType.OpenChevron))
        {
            return;
        }

        ConsumeToken();
        List<GenericTypeDeclaration> genericTypes = [];
        while (!CheckTokenType(TokenType.CloseChevron, 1))
        {
            var typeDeclaration = new GenericTypeDeclaration
            {
                GenericIdentifier = ConsumeIfOfType("Type declaration", TokenType.Ident).Value!
            };
            ParseUpperBound(typeDeclaration);
            genericTypes.Add(typeDeclaration);
            ConsumeIfOfType("comma", TokenType.Comma);
        }

        var finalTypeDeclaration = new GenericTypeDeclaration
        {
            GenericIdentifier = ConsumeIfOfType("Type declaration", TokenType.Ident).Value!
        };
        ParseUpperBound(finalTypeDeclaration);
        genericTypes.Add(finalTypeDeclaration);
        
        ConsumeIfOfType("Closing chevron", TokenType.CloseChevron);
        funcOrClass.SetGenericTypes(genericTypes);
    }

    private void ParseUpperBound(GenericTypeDeclaration typeDeclaration)
    {
        var typeParser = new TypeParser(_tokens, _filePosition, _symbolTableBuilder);
        if (!CheckTokenType(TokenType.Extends)) return;
        
        ConsumeIfOfType("", TokenType.Extends);
        typeDeclaration.UpperBounds.Add(typeParser.ParseComplexTypDeclaration());
        while (CheckTokenType(TokenType.BitAnd))
        {
            ConsumeToken(); // consume &
            typeDeclaration.UpperBounds.Add(typeParser.ParseComplexTypDeclaration());        
        }
    }
}