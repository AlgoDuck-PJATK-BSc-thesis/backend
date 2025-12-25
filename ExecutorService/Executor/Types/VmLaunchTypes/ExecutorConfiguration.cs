using Microsoft.Extensions.Options;

namespace ExecutorService.Executor.Types.VmLaunchTypes;

public class ExecutorConfiguration
{
    public const string SectionName = "Executor";
    
    public VmTimeouts Timeouts { get; set; } = new();
    public VmResources Resources { get; set; } = new();
    public PoolConfiguration Pool { get; set; } = new();
    public ClusterLimits Cluster { get; set; } = new();
    public ExecutionLimits Limits { get; set; } = new();
}

public class VmTimeouts
{
    public TimeSpan QueryTimeout { get; set; } = TimeSpan.FromSeconds(15);
    
    public TimeSpan ResourceRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    public TimeSpan CompilationTimeout { get; set; } = TimeSpan.FromSeconds(60);
    
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    public TimeSpan VmLaunchTimeout { get; set; } = TimeSpan.FromSeconds(60);
}

public class VmResources
{
    public int ExecutorVcpuCount { get; set; } = 1;
    public int ExecutorMemoryMb { get; set; } = 256;
    
    public int CompilerVcpuCount { get; set; } = 2;
    public int CompilerMemoryMb { get; set; } = 2048;
}

public class PoolConfiguration
{
    public TimeSpan TrackingPeriod { get; set; } = TimeSpan.FromMinutes(10);
    
    public TimeSpan PollingFrequency { get; set; } = TimeSpan.FromSeconds(15);
    
    public int DefaultCompilerCacheTarget { get; set; } = 1;
    
    public int DefaultExecutorCacheTarget { get; set; } = 5;
    
    public int OrphanPoolSize { get; set; } = 5;
}

public class ClusterLimits
{
    public int TotalMemoryMb { get; set; } = 8192;
    
    public int TotalVcpuCount { get; set; } = 8;
    
    public double MaxVcpuOversubscription { get; set; } = 1.5;
}

public class ExecutionLimits
{
    
    public int MaxFileCount { get; set; } = 10;
    
    public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.FromSeconds(30);
    
    public int MaxUserMemoryMb { get; set; } = 128;
}

public static class ExecutorConfigurationExtensions
{
    public static IServiceCollection AddExecutorConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<ExecutorConfiguration>(
            configuration.GetSection(ExecutorConfiguration.SectionName));
        
        services.AddSingleton(sp => 
            sp.GetRequiredService<IOptions<ExecutorConfiguration>>().Value);
        
        return services;
    }
}