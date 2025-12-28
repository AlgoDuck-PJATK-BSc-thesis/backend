using System.Text.Json;

namespace AlgoDuckShared;

public static class DefaultJsonSerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(){ PropertyNameCaseInsensitive = true };
    
    public static T? Deserialize<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
    }
}