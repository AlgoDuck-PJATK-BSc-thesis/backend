namespace AlgoDuck.Shared.Http;

public class PageData<T>
{
    public int CurrPage { get; set; } // wonder how well dotnet serializes uints
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int? PrevCursor { get; set; }
    public int? NextCursor { get; set; }
    public ICollection<T> Items { get; set; } = [];
}
