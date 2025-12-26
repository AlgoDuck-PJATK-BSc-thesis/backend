using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;
using OneOf;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;

public abstract class Symbol
{
    public required string Name { get; set; }
    public Scope? DeclaringScope { get; set; }
}

public class VariableSymbol : Symbol
{
    public required AstNodeScopeMemberVar ScopeMemberVar { get; set; }
    public required OneOf<MemberType, ArrayType, ComplexTypeDeclaration> SymbolType { get; set; }
}

public class TypeSymbol<T> : Symbol where T :  BaseType<T>
{
    public BaseType<T>? AssociatedType { get; set; } = null;
}

public class MethodSymbol<T> : Symbol where T :  BaseType<T>
{
    public required AstNodeMemberFunc<T> AssociatedMethod { get; set; }
}

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

    public List<VariableSymbol> GetVariables()
    {
        return Symbols.Values.OfType<VariableSymbol>().ToList();
    }

    public List<TypeSymbol<T>> GetTypes<T>() where T: BaseType<T>
    {
        return Symbols.Values.OfType<TypeSymbol<T>>().ToList();
    }
    
    public List<MethodSymbol<T>> GetMethods<T>() where T : BaseType<T>
    {
        return Symbols.Values.OfType<MethodSymbol<T>>().ToList();
    }
    

    public void PrintAllSymbols()
    {
        foreach (var keyValuePair in Symbols)
        {
            Console.WriteLine(keyValuePair.Key);
        }
    }

    public Symbol? GetSymbol(string name)
    {
        return Symbols.TryGetValue(name, out var symbol) ? symbol : Parent?.GetSymbol(name);
    }

    public static bool IsType(Symbol? symbol)
    {
        return symbol is TypeSymbol<AstNodeClass> or TypeSymbol<AstNodeEnum> or TypeSymbol<AstNodeInterface>;
    }

    public bool IsType(string name)
    {
        return IsType(GetSymbol(name));
    }
}