namespace AlgoDuck.Modules.Cohort.Shared.Utils;

internal sealed class ModerationApiRequest
{
    public string Model { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
}

internal sealed class ModerationApiResponse
{
    public string Id { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public List<ModerationResult>? Results { get; set; }
}

internal sealed class ModerationResult
{
    public bool Flagged { get; set; }
    public ModerationCategories Categories { get; set; } = new();
    public ModerationCategoryScores CategoryScores { get; set; } = new();
}

internal sealed class ModerationCategories
{
    public bool Hate { get; set; }
    public bool HateThreatening { get; set; }
    public bool Harassment { get; set; }
    public bool HarassmentThreatening { get; set; }
    public bool SelfHarm { get; set; }
    public bool SelfHarmIntent { get; set; }
    public bool SelfHarmInstructions { get; set; }
    public bool Sexual { get; set; }
    public bool SexualMinors { get; set; }
    public bool Violence { get; set; }
    public bool ViolenceGraphic { get; set; }
}

internal sealed class ModerationCategoryScores
{
    public double Hate { get; set; }
    public double HateThreatening { get; set; }
    public double Harassment { get; set; }
    public double HarassmentThreatening { get; set; }
    public double SelfHarm { get; set; }
    public double SelfHarmIntent { get; set; }
    public double SelfHarmInstructions { get; set; }
    public double Sexual { get; set; }
    public double SexualMinors { get; set; }
    public double Violence { get; set; }
    public double ViolenceGraphic { get; set; }
}