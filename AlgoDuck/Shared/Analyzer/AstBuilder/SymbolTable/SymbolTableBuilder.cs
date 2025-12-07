namespace AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

public class SymbolTableBuilder
{
    internal Scope GlobalScope { get; private set; } = new();
    internal Scope CurrentScope { get; private set; }

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

        builtInTypes.ForEach(s => GlobalScope.DefineSymbol(new TypeSymbol
        {
            Name = s
        }));
    }
    
    public void EnterScope()
    {
        CurrentScope = new Scope
        {
            Parent = CurrentScope
        };
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