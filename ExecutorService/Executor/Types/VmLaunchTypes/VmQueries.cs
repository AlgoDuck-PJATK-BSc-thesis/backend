using System.Text.Json.Serialization;

namespace ExecutorService.Executor.Types.VmLaunchTypes;

public class VmJobRequestInterface<T> where T : VmPayload
{
    public required Guid JobId { get; set; }
    public required T Payload { get; set; }
}

public class VmJobRequest<T> where T : VmPayload
{
    public required Guid JobId { get; set; }
    public required T Payload { get; set; }
    public required Guid VmId { get; set; }
}


[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(VmCompilationPayload), "comp")]
[JsonDerivedType(typeof(VmExecutionPayload), "exec")]
[JsonDerivedType(typeof(VmHealthCheckPayload), "health")]

public abstract class VmPayload;
public class VmCompilationPayload : VmPayload
{
    public required Guid JobId { get; set; }
    public required Dictionary<string, string> SrcFiles { get; set; }
}

public class VmExecutionPayload : VmPayload
{
    public required string Entrypoint { get; set; }
    public Dictionary<string, string> ClientSrc { get; set; } = [];
}

public class VmHealthCheckPayload : VmPayload
{
    public ICollection<string> FilesToCheck { get; set; } = [];
}

public abstract class VmInputResponse;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(VmCompilationSuccess), "ok")]
[JsonDerivedType(typeof(VmCompilationFailure), "err")]
public abstract class VmCompilationResponse : VmInputResponse;

public class VmCompilationSuccess : VmCompilationResponse
{
    public required Dictionary<string, string> Body { get; set; }
}

public class VmCompilationFailure : VmCompilationResponse
{
    public required string Body { get; set; } = string.Empty;
}

public class VmExecutionResponse : VmInputResponse
{
    public string Out { get; set; } = string.Empty;
    public string Err { get; set; } = string.Empty;
    public string ExitCode { get; set; } = string.Empty;
    public string StartNs { get; set; }  = string.Empty;
    public string EndNs { get; set; }  = string.Empty;
    public string MaxMemoryKb { get; set; }  = string.Empty;
    
}

public class VmCompilerHealthCheckResponse : VmInputResponse
{
    public Dictionary<string, string> FileHashes { get; set; } = [];
}