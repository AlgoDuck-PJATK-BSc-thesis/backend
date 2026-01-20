namespace AlgoDuck.Modules.Auth.Commands.ApiKeys.GenerateApiKey;

public sealed class GenerateApiKeyDto
{
    public string Name { get; set; } = string.Empty;

    public int? LifetimeDays { get; set; }
}