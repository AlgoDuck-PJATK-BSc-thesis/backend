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
        var executeResultDto = DefaultJsonSerializer.Deserialize<ExecuteResponse>(resultRaw);
        if (executeResultDto == null) throw new ExecutorNullResponseException();
        
        return executeResultDto;
    }
}

public class ExecutorNullResponseException(string? msg = "") : Exception(msg); 