using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor._ExecutorUtils;
using ExecutorService.Executor.Configs;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

public interface ICompilationHandler
{
    public Task<CompileResultDto> CompileAsync(string codeB64, string classname);
}

public sealed class CompilationHandler : ICompilationHandler, IDisposable
{
    private static readonly JsonSerializerOptions JsonSerializerConfiguration = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    
    private readonly Channel<CompileTask> _tasksToDispatch;
    private readonly ChannelWriter<CompileTask> _taskWriter;
    private readonly ChannelReader<CompileTask> _taskReader;
    
    private readonly Channel<int> _availableContainerPorts;
    private readonly ChannelWriter<int> _portWriter;
    private readonly ChannelReader<int> _portReader;
    
    private readonly HttpClient _client;
    
    public CompilationHandler()
    {
        var config = YmlConfigReader.ReadExecutorYmlConfig();
        
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        _tasksToDispatch = Channel.CreateBounded<CompileTask>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
        _taskWriter = _tasksToDispatch.Writer;
        _taskReader = _tasksToDispatch.Reader;
        
        _availableContainerPorts = Channel.CreateUnbounded<int>();
        _portWriter = _availableContainerPorts.Writer;
        _portReader = _availableContainerPorts.Reader;

        for (var i = 0; i < config.COMPILATION_HANDLER.BASE_COUNT; i++)
        {
            var containerPort = config.COMPILATION_HANDLER.BASE_PORT + i;
            _portWriter.TryWrite(containerPort);
        }

        for (var i = 0; i < config.COMPILATION_HANDLER.THREAD_COUNT; i++)
        {
            Task.Run(DispatchContainers);
        }
    }

    public async Task<CompileResultDto> CompileAsync(string codeB64, string classname)
    {
        TaskCompletionSource<CompileResultDto> compileTask = new();
        await _taskWriter.WriteAsync(new CompileTask(codeB64, classname, compileTask));
        return await compileTask.Task;
    }

    private async Task DispatchContainers()
    {
        while (true)
        {
            var task = await GetCompilationTask();
            var port = await GetAvailableContainerPort();

            var url = $"http://compiler{port}:5137/compile";
            
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new CompileRequestDto(task.Code, task.ClassName)),
                        Encoding.UTF8, "application/json")
                };
                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {

                    var raw = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CompileResultDto>(raw, JsonSerializerConfiguration);
                    if (result == null)
                    {
                        throw new CompilationException("Something went wrong during compilation");
                    }
                    task.Tcs.SetResult(result);
                }
                else if(response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var rawResponse = await response.Content.ReadAsStringAsync();
                    var exceptionContents = new CompilationException(rawResponse);
                    task.Tcs.SetException(exceptionContents);
                }
                else
                {
                    var exceptionContents = new UnknownCompilationException(":(");
                    task.Tcs.SetException(exceptionContents);
                }
            }
            catch (Exception ex)
            {
                task.Tcs.SetException(ex);
            }
            finally
            {
                _portWriter.TryWrite(port);
            }
        }
    }

    private async Task<CompileTask> GetCompilationTask()
    {
        while (await _taskReader.WaitToReadAsync())
        {
            if (_taskReader.TryRead(out var task))
            {
                return task;
            }
        }

        throw new CompilationHandlerChannelReadException("Could not fetch task");
    }

    private async Task<int> GetAvailableContainerPort()
    {
        while (await _portReader.WaitToReadAsync())
        {
            if (_portReader.TryRead(out var task))
            {
                return task;
            } 
            await Task.Delay(10);
        }
        throw new CompilationHandlerChannelReadException("Could not fetch task");
    }
    
    private static async Task DeployCompilerContainerAsync(int port) // made to allow dynamic runtime scaling
    {
        var launchProcess = CreateLaunchProcess(port);
        launchProcess.Start();
        await launchProcess.WaitForExitAsync();
    }
    
    private static Process CreateLaunchProcess(int port)
    {
        return  new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/app-scripts/deploy-compiler.sh {port}", 
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
    }

    public void Dispose()
    {
        Console.WriteLine("Shutting down compile cluster. Please wait...");
        while (_portReader.TryRead(out var port))
        {
            var url = $"http://warden:7139/container/{port}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            _client.Send(request);
        }
    }
}
