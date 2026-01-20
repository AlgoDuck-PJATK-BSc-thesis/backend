using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

public class SymbolTableBuilder
{
    public int RecursionDepth { get; set; } = 0;
    public const int MaxRecursionDepth = 100;
    internal Scope GlobalScope { get; private set; } = new();
    internal Scope CurrentScope { get; private set; }

    
    public int ParseOperations { get; set; } = 0;
    public const int MaxParseOperations = 100000;

    public void IncrementParseOps()
    {
        if (++ParseOperations > MaxParseOperations)
        {
            throw new JavaParseComplexityExceededException("Parse complexity limit exceeded");
        }
    }
    public static SymbolTableBuilder Create(out SymbolTableBuilder builder)
    {
        builder = new SymbolTableBuilder();
        return builder;
    }
    
    public SymbolTableBuilder()
    {
        CurrentScope = GlobalScope;
        DefineBuiltInTypes();
    }
    private void DefineBuiltInTypes()
    {
        List<string> builtInTypes =
        [
            "byte", "short", "int", "long", "float", "double", "char", "boolean", "String", "Object", "void"
        ];

        builtInTypes.ForEach(s => GlobalScope.DefineSymbol(new TypeSymbol<AstNodeClass>
        {
            Name = s
        }));
    }
    
    public void EnterScope()
    {
        var newScope = new Scope
        {
            Parent = CurrentScope
        };
        CurrentScope.Children.Add(newScope);
        CurrentScope = newScope;
    }

    public void ExitScope()
    {
        if (CurrentScope.Parent != null)
        {
            CurrentScope = CurrentScope.Parent;
        }
    }

    public bool DefineSymbol(Symbol symbol)
    {
        return CurrentScope.DefineSymbol(symbol);
    }
    
    public bool IsType(string name)
    {
        return CurrentScope.IsType(name);
    }

    public Symbol? Resolve(string name)
    {
        return CurrentScope.GetSymbol(name);
    }
    
}

public class JavaParseComplexityExceededException(string? message = "") : Exception(message);
