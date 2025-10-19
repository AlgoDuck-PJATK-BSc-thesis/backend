using System.Text.Json.Serialization;

namespace ExecutorService.Executor.Types.VmLaunchTypes;

public abstract class VmInputQuery;

public abstract class VmInputResponse;

public class VmCompilationQueryContent
{
    public string SrcCodeB64 { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Guid ExecutionId { get; set; }
}

public class VmHealthCheckContent;

public class VmCompilationQuery<T> : VmInputQuery
{
    public string Endpoint { get; set; } = "health_check";
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public T? Content { get; set; }
    public string Ctype { get; set; } = "application/json";
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(VmCompilationSuccess), "success")]
[JsonDerivedType(typeof(VmCompilationFailure), "error")]
public abstract class VmCompilationResponse : VmInputResponse;

public class VmCompilationSuccess : VmCompilationResponse
{
    public string Entrypoint { get; set; } = string.Empty;
    public Dictionary<string, string> GeneratedClassFiles { get; set; } = [];
}

public class VmCompilationFailure : VmCompilationResponse
{
    public string ErrorMsg { get; set; } = string.Empty;
}

public class VmExecutionQuery : VmInputQuery
{
    public VmExecutionQuery(VmCompilationSuccess compilationResponse)
    {
        Entrypoint = compilationResponse.Entrypoint;
        GeneratedClassFiles = compilationResponse.GeneratedClassFiles;
    }
    public string Entrypoint { get; set; }
    public Dictionary<string, string> GeneratedClassFiles { get; set; }
}

public class VmExecutionResponse : VmInputResponse
{
    public string Out { get; set; } = string.Empty;
    public string Err { get; set; } = string.Empty;
}

public class VmCompilerHealthCheckResponse : VmInputResponse
{
    public Dictionary<string, string> FileHashes { get; set; } = [];
}