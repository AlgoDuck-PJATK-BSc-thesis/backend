using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface IGenericParser
{
    public void ParseGenericDeclaration(IGenericSettable funcOrClass);

}