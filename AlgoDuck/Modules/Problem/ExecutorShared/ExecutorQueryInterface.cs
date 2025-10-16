using System.Text;
using System.Text.Json;
using AlgoDuckShared;
using AlgoDuckShared.Executor.SharedTypes;

namespace AlgoDuck.Modules.Problem.ExecutorShared;

internal interface IExecutorQueryInterface
{
    internal Task<ExecuteResponse> ExecuteAsync(ExecuteRequest executeRequest);
}

internal class ExecutorQueryInterface(IHttpClientFactory httpClientFactory) : IExecutorQueryInterface
{
    public async Task<ExecuteResponse> ExecuteAsync(ExecuteRequest executeRequest)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(executeRequest), Encoding.UTF8, "application/json")
        };
    
        using var client = httpClientFactory.CreateClient("executor");
        var response = await client.SendAsync(request);
        var resultRaw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = DefaultJsonSerializer.Deserialize<ExecutorErrorResponse>(resultRaw);
            if (errorResponse == null) throw new ExecutorNullResponseException("Failed to deserialize error response");
            return errorResponse;
        }
    
        ExecuteResponse? executeResultDto = executeRequest switch
        {
            SubmitExecuteRequest => DefaultJsonSerializer.Deserialize<SubmitExecuteResponse>(resultRaw),
            DryExecuteRequest => DefaultJsonSerializer.Deserialize<DryExecuteResponse>(resultRaw),
            _ => throw new NotSupportedException($"Request type {executeRequest.GetType().Name} not supported")
        };
    
        if (executeResultDto == null) throw new ExecutorNullResponseException("Failed to deserialize success response");
    
        return executeResultDto;
    }
}

public class ExecutorNullResponseException(string? msg = "") : Exception(msg); 