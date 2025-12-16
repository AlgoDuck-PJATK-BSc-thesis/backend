public class ComplexTypeDeclaration
{
    public string Identifier { get; set; } = string.Empty;
    public List<GenericInitialization>? GenericInitializations { get; set; }

    public override string ToString()
    {
        if (GenericInitializations == null || GenericInitializations.Count == 0)
        {
            return Identifier;
        }
        return $"{Identifier}<{string.Join(", ", GenericInitializations)}>";
    }
}

public class GenericInitialization
{
    public ComplexTypeDeclaration? Identifier { get; set; }
    public bool IsWildCard { get; set; } = false;
    public ComplexTypeDeclaration? SupersType { get; set; }
    public List<ComplexTypeDeclaration>? ExtendsTypes { get; set; }

    public override string ToString()
    {
        if (IsWildCard)
        {
            if (SupersType != null)
            {
                return $"? super {SupersType}";
            }
            return ExtendsTypes is { Count: > 0 } ? $"? extends {string.Join(" & ", ExtendsTypes)}" : "?";
        }

        if (Identifier == null)
        {
            return string.Empty;
        }
        if (ExtendsTypes is { Count: > 0 })
        {
            return $"{Identifier} extends {string.Join(" & ", ExtendsTypes)}";
        }
        
        return Identifier.ToString() ?? string.Empty;
    }
}