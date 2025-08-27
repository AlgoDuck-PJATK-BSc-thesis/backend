using OneOf;

using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Interfaces;

namespace ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodeCompilationUnit
{
    public List<AstNodeImport> Imports { get; set; } = [];
    public AstNodePackage? Package;
    public List<OneOf<AstNodeClass, AstNodeInterface>> CompilationUnitTopLevelStatements { get; set; } = [];
}