namespace WebApplication1.Shared.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized")
        : base(message, 401) { }
}