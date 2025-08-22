using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
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
        List<Token> genericTypes = [];
        while (!CheckTokenType(TokenType.CloseChevron, 1))
        {
            genericTypes.Add(ConsumeIfOfType(TokenType.Ident, "Type declaration"));
            ConsumeIfOfType(TokenType.Comma, "comma");
        }
        genericTypes.Add(ConsumeIfOfType(TokenType.Ident, "Type declaration"));
        ConsumeIfOfType(TokenType.CloseChevron, "Closing chevron");
        
        funcOrClass.SetGenericTypes(genericTypes);
    }
}