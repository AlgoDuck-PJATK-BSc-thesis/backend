using System.Net;

namespace ExecutorService.Errors;

public class ExceptionReponseDto(HttpStatusCode statusCode, string errMsg)
{
    public HttpStatusCode StatusCode => statusCode;
    public string ErrMsg => errMsg;
}