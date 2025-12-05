namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public sealed class PermissionException : AuthException
{
    public PermissionException(string message)
        : base("permission_denied", message)
    {
    }

    public PermissionException(string message, Exception? innerException)
        : base("permission_denied", message, innerException)
    {
    }
}