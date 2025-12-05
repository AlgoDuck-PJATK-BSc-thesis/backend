namespace AlgoDuck.Modules.Auth.Commands.GenerateApiKey;

public sealed class GenerateApiKeyDto
{
    public string Name { get; set; } = string.Empty;

    public int? LifetimeDays { get; set; }
}