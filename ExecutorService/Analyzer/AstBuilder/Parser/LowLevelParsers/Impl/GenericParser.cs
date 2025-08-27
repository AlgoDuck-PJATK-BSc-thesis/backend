using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer.AstBuilder.Parser.CoreParsers;
using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;

public class GenericParser(List<Token> tokens, FilePosition filePosition) :
    ParserCore(tokens, filePosition),
    IGenericParser
{
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
                GenericIdentifier = ConsumeIfOfType(TokenType.Ident, "Type declaration").Value!
            };
            ParseUpperBound(typeDeclaration);
            genericTypes.Add(typeDeclaration);
            ConsumeIfOfType(TokenType.Comma, "comma");
        }

        var finalTypeDeclaration = new GenericTypeDeclaration
        {
            GenericIdentifier = ConsumeIfOfType(TokenType.Ident, "Type declaration").Value!
        };
        ParseUpperBound(finalTypeDeclaration);
        genericTypes.Add(finalTypeDeclaration);
        
        ConsumeIfOfType(TokenType.CloseChevron, "Closing chevron");
        funcOrClass.SetGenericTypes(genericTypes);
    }

    private void ParseUpperBound(GenericTypeDeclaration typeDeclaration)
    {
        var typeParser = new TypeParser(tokens, filePosition);
        if (!CheckTokenType(TokenType.Extends)) return;
        
        ConsumeIfOfType(TokenType.Extends, "");
        typeDeclaration.UpperBounds.Add(typeParser.ParseComplexTypDeclaration());
        while (CheckTokenType(TokenType.And))
        {
            ConsumeToken(); // consume &
            typeDeclaration.UpperBounds.Add(typeParser.ParseComplexTypDeclaration());        
        }
    }
}