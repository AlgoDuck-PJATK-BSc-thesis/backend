namespace WebApplication1.Shared.Exceptions;

public class InternalServerErrorException : AppException
{
    public InternalServerErrorException(string message = "An unexpected error occurred")
        : base(message, 500) { }
}