using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebApplication1.Modules.ProblemModule.DTOs;
using WebApplication1.Modules.ProblemModule.DTOs.ExecutorDtos;
using WebApplication1.Modules.ProblemModule.Interfaces;
using WebApplication1.Shared.Exceptions;

namespace WebApplication1.Modules.ProblemModule.Services;

public class CodeExecutorService : IExecutorService
{
    private const string Hostname = "executor";
    
    private static readonly string ExecutorPort = Environment.GetEnvironmentVariable("EXECUTOR_PORT") ?? "1337"; 
    private static readonly string BaseUrl = $"http://{Hostname}:{ExecutorPort}";
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(){ PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public CodeExecutorService()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    

    public async Task<ExecuteResultDto> DryExecuteCode(DryExecuteRequestDto executeRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/execute/dry")
        {
            Content = new StringContent(JsonSerializer.Serialize(executeRequest), Encoding.UTF8, "application/json")
        };
        
        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorResultRaw = await response.Content.ReadAsStringAsync();
            var errorResult = JsonSerializer.Deserialize<ExecutorExceptionResponseDto>(errorResultRaw, JsonSerializerOptions);

            if (errorResult == null) throw new InternalServerErrorException("something went wrong");
            return new ExecuteResultDto
            {
                StdError = errorResult.ErrMsg
            };
        }
    
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ExecuteResultDto>(content, JsonSerializerOptions);


        if (result == null) throw new Exception(""); // TODO make this a custom exception
        return result;
    }

    // repeated because in the future I may end up using different return types who knows
    public async Task<ExecuteResultDto> FullExecuteCode(ExecuteRequestDto executeRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/execute/full")
        {
            Content = new StringContent(JsonSerializer.Serialize(executeRequest), Encoding.UTF8, "application/json")
        };
        
        var response = await _client.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorResultRaw = await response.Content.ReadAsStringAsync();
            var errorResult = JsonSerializer.Deserialize<ExecutorExceptionResponseDto>(errorResultRaw, JsonSerializerOptions);

            if (errorResult == null) throw new InternalServerErrorException("something went wrong");
            return new ExecuteResultDto
            {
                StdError = errorResult.ErrMsg
            };
        }
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ExecuteResultDto>(content, JsonSerializerOptions);
        
        if (result == null) throw new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        
        return result;
    }
}