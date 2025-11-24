using System.Text;
using System.Text.Json;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuckShared;
using AlgoDuckShared.Executor.SharedTypes;

namespace AlgoDuck.Modules.Problem.ExecutorShared;

internal interface IExecutorQueryInterface
{
    internal Task<ExecutionResponse> ExecuteAsync(ExecutionRequest executeRequest);
}
public class ExecutionRequest
{
    public required Dictionary<string, string> JavaFiles { get; set; }
}


public class ExecutionResponse
{
    public string Out { get; set; } = string.Empty;
    public string Err { get; set; } = string.Empty;
}

internal class ExecutorQueryInterface(IHttpClientFactory httpClientFactory) : IExecutorQueryInterface
{
    public async Task<ExecutionResponse> ExecuteAsync(ExecutionRequest executeRequest)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(executeRequest), Encoding.UTF8, "application/json")
        };
    
        using var client = httpClientFactory.CreateClient("executor");
        var response = await client.SendAsync(request);
        var resultRaw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return DefaultJsonSerializer.Deserialize<ExecutionResponse>(resultRaw)
                   ?? throw new ExecutorNullResponseException("Failed to deserialize error response");
        }
        
        var errorResponse = DefaultJsonSerializer.Deserialize<ExecutorErrorResponse>(resultRaw) 
                            ?? throw new ExecutorNullResponseException("Failed to deserialize error response");
        throw new ExecutionOutputNotFoundException(errorResponse.ErrMsg);
    }
}

public class ExecutorNullResponseException(string? msg = "") : Exception(msg); 