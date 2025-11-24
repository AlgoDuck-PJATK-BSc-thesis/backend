using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public interface IGenericParser
{
    public void ParseGenericDeclaration(IGenericSettable funcOrClass);

}