using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using OneOf;

namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;

public class AstNodeCompilationUnit
{
    public List<AstNodeImport> Imports { get; set; } = [];
    public AstNodePackage? Package;
    public List<OneOf<AstNodeClass, AstNodeInterface>> CompilationUnitTopLevelStatements { get; set; } = [];
}