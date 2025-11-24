using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.ModelsExternal;

public class ProblemS3PartialInfo
{
    public required Guid ProblemId { get; init; }
    public required SupportedLanguage CountryCode { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}