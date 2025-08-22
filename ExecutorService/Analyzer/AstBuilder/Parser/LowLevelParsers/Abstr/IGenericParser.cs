using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface IGenericParser
{
    public void ParseGenericDeclaration(IGenericSettable funcOrClass);

}