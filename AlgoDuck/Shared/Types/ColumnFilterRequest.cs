using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Shared.Types;

public class ColumnFilterRequest<TEnum> where TEnum : struct, Enum
{
    [FromQuery(Name = "columns")]
    public string? FieldsRaw { get; set; }

    public HashSet<TEnum> Fields =>
        FieldsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => Enum.TryParse<TEnum>(f, ignoreCase: true, out var val) ? val : (TEnum?)null)
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .ToHashSet()
        ?? [];

    [FromQuery(Name = "orderBy")]
    public string? OrderByRaw { get; set; }

    public TEnum? OrderBy => 
        Enum.TryParse<TEnum>(OrderByRaw, ignoreCase: true, out var val) ? val : null;
}