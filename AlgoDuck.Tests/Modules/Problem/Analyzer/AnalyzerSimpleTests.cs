using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;

namespace AlgoDuck.Tests.Modules.Problem.Analyzer;

public class AnalyzerSimpleTests
{
    [Fact]
    public void AnalyzeUserCode_WithValidMainMethod_ReturnsCorrectMainMethodIndices()
    {
        var javaCode = @"
public class Main {
    public static void main(String[] args) {
        System.out.println(""Hello"");
    }
}";
        var analyzer = new AnalyzerSimple(new StringBuilder(javaCode));

        var result = analyzer.AnalyzeUserCode(ExecutionStyle.Execution);

        Assert.True(result.PassedValidation);
        Assert.NotNull(result.MainMethodIndices);
        Assert.Equal("Main", result.MainClassName);
    }

    [Fact]
    public void AnalyzeUserCode_WithMissingMainMethod_InsertsMainMethod()
    {
        var javaCode = @"
public class Main {
    public static int add(int a, int b) {
        return a + b;
    }
}";
        var analyzer = new AnalyzerSimple(new StringBuilder(javaCode));

        var result = analyzer.AnalyzeUserCode(ExecutionStyle.Execution);

        Assert.NotNull(result.MainMethodIndices);
    }


}