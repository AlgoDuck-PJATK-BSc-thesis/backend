using System.Text;
using System.Text.Json;
using ExecutorService.Executor._ExecutorUtils;
using WebApplication1.Modules.ProblemModule.Interfaces;

namespace WebApplication1.Modules.ProblemModule.Services;

public class ExecutorService(HttpClient client) : IExecutorService
{
    private readonly string _baseUrl = "https://localhost:1337";

    public async Task<ExecuteResultDto> DryExecuteCode(ExecuteRequestDto executeRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/execute/dry")
        {
            Content = new StringContent(JsonSerializer.Serialize(executeRequest), Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ExecuteResultDto>(content);
            if (result != null)
            {
                return result;
            }
        }
        throw new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"); 
    }

    // repeated because in the future I may end up using different return types who knows
    public async Task<ExecuteResultDto> FullExecuteCode(ExecuteRequestDto executeRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/execute/dry")
        {
            Content = new StringContent(JsonSerializer.Serialize(executeRequest), Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ExecuteResultDto>(content);
            if (result != null)
            {
                return result;
            }
        }
        throw new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"); 
    }
}