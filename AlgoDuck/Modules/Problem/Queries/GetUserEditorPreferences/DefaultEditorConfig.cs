namespace AlgoDuck.Modules.Problem.Queries.GetUserEditorPreferences;

public class EditorDefaultConfig
{
    public required Guid DefaultLayoutId { get; set; }
    public required Guid DefaultThemeId { get; set; }
    public required int DefaultFontSize { get; set; }
}