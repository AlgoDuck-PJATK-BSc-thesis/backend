using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Parser.TopLevelParsers.Abstr;

public interface IEnumParser
{
    public AstNodeEnum ParseEnum();
}