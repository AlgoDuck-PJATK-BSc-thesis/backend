using System.Text;
using System.Text.Json;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuckShared;

namespace AlgoDuck.Modules.Problem.Shared;

internal interface IExecutorQueryInterface
{
    internal Task<ExecutionResponseRabbit> ExecuteAsync(ExecutionRequest executeRequest);
}
public class ExecutionRequest
{
    public required Dictionary<string, string> JavaFiles { get; set; }
}

internal class ExecutorQueryInterface(IHttpClientFactory httpClientFactory) : IExecutorQueryInterface
{
    public async Task<ExecutionResponseRabbit> ExecuteAsync(ExecutionRequest executeRequest)
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
            return DefaultJsonSerializer.Deserialize<ExecutionResponseRabbit>(resultRaw)
                   ?? throw new ExecutorNullResponseException("Failed to deserialize error response");
        }
        
        // var errorResponse = DefaultJsonSerializer.Deserialize<ExecutorErrorResponse>(resultRaw) 
        //                     ?? throw new ExecutorNullResponseException("Failed to deserialize error response");
        throw new ExecutionOutputNotFoundException("errorResponse.ErrMsg");
    }
}

public class ExecutorNullResponseException(string? msg = "") : Exception(msg); 