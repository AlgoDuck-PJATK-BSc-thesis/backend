namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;

public class JavaSyntaxException(string? message) : Exception(message); //error mainly for debugging as the end user will get standard java error messages either way 