using System.Net;
using System.Text.Json.Serialization;

namespace AlgoDuckShared.Executor.SharedTypes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SubmitExecuteRequest), typeDiscriminator: "submit")]
[JsonDerivedType(typeof(DryExecuteRequest), typeDiscriminator: "dry")]
public abstract class ExecuteRequest
{
    public string CodeB64 { get; set; } = string.Empty;
}

public class SubmitExecuteRequest : ExecuteRequest
{
    public Guid ExerciseId { get; set; } = Guid.Empty;
}

public class DryExecuteRequest : ExecuteRequest;



[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(SubmitExecuteResponse), typeDiscriminator: "submit")]
[JsonDerivedType(typeof(DryExecuteResponse), typeDiscriminator: "dry")]
[JsonDerivedType(typeof(ExecutorErrorResponse), typeDiscriminator: "error")]
public abstract class ExecuteResponse;

public class SubmitExecuteResponse : ExecuteResponse
{
    public string StdOutput { get; set; } = string.Empty;
    public string StdError { get; set; } = string.Empty;
    public List<TestResultDto> TestResults { get; set; } = [];
    public int ExecutionTime { get; set; } = 0;
}

public class DryExecuteResponse : ExecuteResponse
{
    public string StdOutput { get; set; } = string.Empty;
    public string StdError { get; set; } = string.Empty;
    public int ExecutionTime { get; set; } = 0;
}

public class ExecutorErrorResponse : ExecuteResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string ErrMsg { get; set; } = string.Empty;
}

public class TestResultDto
{
    public string TestId { get; set; } = string.Empty;
    public bool IsTestPassed { get; set; } = false;
}