using System.Net;

namespace AlgoDuck.Modules.Problem.DTOs.ExecutorDtos;

public class ExecutorExceptionResponseDto(HttpStatusCode statusCode, string errMsg)
{
    public HttpStatusCode StatusCode => statusCode;
    public string ErrMsg => errMsg;
}