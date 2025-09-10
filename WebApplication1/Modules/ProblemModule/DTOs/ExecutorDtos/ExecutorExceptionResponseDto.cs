using System.Net;

namespace WebApplication1.Modules.ProblemModule.DTOs.ExecutorDtos;

public class ExecutorExceptionResponseDto(HttpStatusCode statusCode, string errMsg)
{
    public HttpStatusCode StatusCode => statusCode;
    public string ErrMsg => errMsg;
}