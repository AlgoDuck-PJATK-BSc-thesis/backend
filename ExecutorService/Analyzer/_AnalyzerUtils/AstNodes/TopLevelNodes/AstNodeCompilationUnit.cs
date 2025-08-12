using OneOf;

using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodeCompilationUnit
{
    public List<AstNodeImport> Imports { get; set; } = [];
    public AstNodePackage? Package;
    public List<OneOf<AstNodeClass>> CompilationUnitTopLevelStatements { get; set; } = [];
}