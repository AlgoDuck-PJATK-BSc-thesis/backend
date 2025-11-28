using System.Text;
using System.Xml.Serialization;

namespace AlgoDuck.Shared.Utilities;

internal static class XmlToObjectParser
{
    internal static async Task<TResult> ReadAndParseFileAsync<TResult>(FileInfo file)
    {
        return await ReadAndParseFileAsync<TResult, TResult>(file);
    }
    
    internal static async Task<TResult> ReadAndParseFileAsync<T, TResult>(FileInfo file) where TResult : T
    {
        if (!file.Extension.Equals(".xml", StringComparison.CurrentCultureIgnoreCase)) throw new InvalidOperationException("Invalid file extension");
        using var reader = file.OpenText();
        var fileContent = new StringBuilder();
        while (await reader.ReadLineAsync() is { } line)
        {
            fileContent.Append(line);
        }
        var xml = ParseXmlString<T>(fileContent.ToString());
        return (TResult) (xml ?? throw new XmlParsingException("Unable to parse xml"));
    }

    internal static TResult? ParseXmlString<TResult>(string xml)
    {
        using var reader = new StringReader(xml.Trim());
        var serializer = new XmlSerializer(typeof(TResult));
        var result = serializer.Deserialize(reader);
        if (result == null) return default;
        return (TResult) result;
    }
}

public class XmlParsingException(string? msg = "") : Exception(msg);
