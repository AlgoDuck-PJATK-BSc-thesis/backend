using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface ITopLevelStatementParser
{
    public void ParseImportsAndPackages(IHasUriSetter statement);
    public OneOf<AstNodeClass, AstNodeInterface> ParseTypeDefinition();
}