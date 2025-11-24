using System.Text.Json.Serialization;

namespace AlgoDuck.ModelsExternal;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum SupportedLanguage
{
    En, Pl
}