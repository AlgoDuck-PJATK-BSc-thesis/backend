using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;

using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

public abstract class Symbol
{
    public required string Name { get; set; }
    public Scope? DeclaringScope { get; set; }
}

public class VariableSymbol : Symbol
{
    public required OneOf<MemberType, ArrayType, ComplexTypeDeclaration> SymbolType { get; set; }
}

public class TypeSymbol : Symbol;

public class MethodSymbol : Symbol;

public class Scope
{
    private Dictionary<string, Symbol> Symbols { get; set; } = [];
    internal List<Scope> Children { get; private set; } = [];
    internal Scope? Parent { get; init; }

    public bool DefineSymbol(Symbol symbol)
    {
        var added = Symbols.TryAdd(symbol.Name, symbol);
        if (added)
        {
            symbol.DeclaringScope = this;
        }
        return added;
    }

    public Symbol? GetSymbol(string name)
    {
        return Symbols.TryGetValue(name, out var symbol) ? symbol : Parent?.GetSymbol(name);
    }

    public static bool IsType(Symbol? symbol)
    {
        return symbol is TypeSymbol;
    }

    public bool IsType(string name)
    {
        return IsType(GetSymbol(name));
    }
}