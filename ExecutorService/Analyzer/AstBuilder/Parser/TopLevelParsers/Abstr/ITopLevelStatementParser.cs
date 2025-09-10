using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using OneOf; 

namespace ExecutorService.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface ITopLevelStatementParser
{
    public void ParseImportsAndPackages(IHasUriSetter statement);
    public OneOf<AstNodeClass, AstNodeInterface> ParseTypeDefinition();
}